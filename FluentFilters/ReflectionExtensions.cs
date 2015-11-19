// ReSharper disable CheckNamespace
namespace System
// ReSharper restore CheckNamespace
{
    using Collections.Generic;
    using Linq.Expressions;
    using Reflection;

    public static class ReflectionExtensions
    {
        #region Fields

        /// <summary>
        /// A cache of delegates generated from expressions.
        /// Reusing these is much, much faster than reflecting the same fields and property getters/setters repeatedly
        /// </summary>
        private static readonly IDictionary<MemberInfo, GetterSetter> DelegateCache = new Dictionary<MemberInfo, GetterSetter>();

        #endregion Fields

        #region Public Methods

        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).PropertyType;
                default:
                    throw new ApplicationException(string.Format("Cannot get type for member type [{0}]", memberInfo.MemberType));
            }
        }

        public static bool HasPropertyOrField(this Type type, string propertyOrFieldName)
        {
            return type.GetPropertyOrFieldInfo(propertyOrFieldName) != null;
        }

        public static MemberInfo GetPropertyOrFieldInfo(this Type type, string propertyOrFieldName)
        {
            return type.GetProperty(propertyOrFieldName) as MemberInfo ?? type.GetField(propertyOrFieldName);
        }

        public static bool CanWrite(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return !((FieldInfo)memberInfo).IsInitOnly;
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).CanWrite;
                default:
                    return false;
            }
        }

        public static bool CanRead(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return true;
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).CanRead;
                default:
                    return false;
            }
        }

        public static bool IsStatic(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).IsStatic;
                case MemberTypes.Method:
                    return ((MethodInfo)memberInfo).IsStatic;
                case MemberTypes.Property:
                    var fieldAccessor = ((PropertyInfo)memberInfo).GetGetMethod() ?? ((PropertyInfo)memberInfo).GetSetMethod();
                    return fieldAccessor != null && IsStatic(fieldAccessor);
                default:
                    return false;
            }
        }

        public static object GetValueForPropertyOrField(this MemberInfo member, object o, object[] index)
        {
            var propInfo = member as PropertyInfo;
            return propInfo != null
              ? propInfo.GetValue(o, index)
              : ((FieldInfo)member).GetValue(o);
        }

        public static void SetValueForPropertyOrField<T>(this MemberInfo member, object o, T value)
        {
            var getterSetter = GetOrCreateDelegates(member);
            getterSetter.InvokeSetter(member, o, value);
        }

        public static TExpression GetExpressionBody<T, TExpression>(this Expression<Func<T>> expression) where TExpression : class
        {
            var result = expression.Body;
            if (!(result is TExpression))
            {
                var msg = string.Format("The expression is not a {0}.", typeof(TExpression).Name);
                throw new ArgumentException(msg, "expression");
            }
            return expression.Body as TExpression;
        }

        public static MemberExpression GetMemberBody<T>(this Expression<Func<T>> member)
        {
            // check for unary
            var unary = member.Body as UnaryExpression;
            if (unary != null)
            {
                return unary.Operand as MemberExpression;
            }

            return GetExpressionBody<T, MemberExpression>(member);
        }

        public static string GetMemberName<T>(this Expression<Func<T>> member)
        {
            return member.GetMemberBody().Member.Name;
        }

        public static string GetMemberPath<T>(this Expression<Func<T>> expression, string pathSeparator = ".")
        {
            MemberExpression memberExpression;

            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression != null)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = expression.Body as MemberExpression;
            }

            if (memberExpression != null)
            {
                return GetMemberPathRecursive(memberExpression, pathSeparator);
            }

            return string.Empty;
        }

        public static object GetSourceObject<T>(this Expression<Func<T>> expression)
        {
            var body = expression.Body;

            var intermediateExpression = body is UnaryExpression
                                           ? ((MemberExpression)((UnaryExpression)body).Operand).Expression
                                           : ((MemberExpression)body).Expression;

            if (intermediateExpression is ConstantExpression)
            {
                return ((ConstantExpression)intermediateExpression).Value;
            }

            return GetSourceObjectRecursive((MemberExpression)intermediateExpression);
        }

        public static T GetValue<T>(this Expression<Func<T>> member)
        {
            MemberExpression memberExpression;
            var unary = member.Body as UnaryExpression;
            if (unary != null)
            {
                memberExpression = unary.Operand as MemberExpression;
            }
            else
            {
                memberExpression = member.Body as MemberExpression;
            }

            GetterSetter getterSetter;
            if (!DelegateCache.TryGetValue(memberExpression.Member, out getterSetter))
            {
                getterSetter = GetOrCreateDelegates(memberExpression.Member);
            }

            return (T)getterSetter.InvokeGetter(memberExpression.Member, member.GetSourceObject());
        }

        public static TResult GetValue<TInput, TResult>(this Expression<Func<TInput, TResult>> member, TInput input)
        {
            return member.Compile().Invoke(input);
        }

        public static void SetValue<T>(this Expression<Func<T>> member, object value)
        {
            var memberSource = member.GetSourceObject();
            var body = member.GetMemberBody();
            SetValueForPropertyOrField(body.Member, memberSource, value);
        }

        #endregion Public Methods

        #region Helpers

        private static string GetMemberPathRecursive(MemberExpression expression, string pathSeparator)
        {
            var parentExpression = expression.Expression;
            if (parentExpression is ConstantExpression)
            {
                return string.Empty;
            }

            var parentMember = parentExpression as MemberExpression;
            if (parentMember != null)
            {
                var parentResult = GetMemberPathRecursive(parentMember, pathSeparator);
                return parentResult == string.Empty
                         ? expression.Member.Name
                         : parentResult + pathSeparator + expression.Member.Name;
            }

            return string.Empty;
        }

        private static object GetSourceObjectRecursive(MemberExpression memberExpression)
        {
            if (memberExpression != null)
            {
                Type type = null;
                object parentObject = null;

                var parentExpression = memberExpression.Expression;
                if (parentExpression is ConstantExpression)
                {
                    var constantExpression = (ConstantExpression)parentExpression;
                    type = constantExpression.Value.GetType();
                    parentObject = constantExpression.Value;
                }
                else if (parentExpression is MemberExpression)
                {
                    parentObject = GetSourceObjectRecursive((MemberExpression)parentExpression);
                    type = parentObject.GetType();
                }

                var prop = memberExpression.Member.Name;

                Func<Type, string, MemberInfo[]> getMember = (t, p) => t.GetMember(p,
                  BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

                var memberInfo = getMember(type, prop);

                // look for private members in base types, if necessary
                while ((memberInfo == null || memberInfo.Length == 0) && type.BaseType != null)
                {
                    type = type.BaseType;
                    memberInfo = getMember(type, prop);
                }

                if (memberInfo != null && memberInfo.Length > 0)
                {
                    return memberInfo[0].GetValue(parentObject);
                }
            }

            return null; // either the member was not found or it is neither a PropertyInfo nor a FieldInfo
        }

        private static object GetValue(this MemberInfo member, object valueSource)
        {
            var getterSetter = GetOrCreateDelegates(member);
            return getterSetter.InvokeGetter(member, valueSource);
        }

        private static readonly MethodInfo ExpressionGetterCreator = typeof(ReflectionExtensions).GetMethod("CreateGetterExpression", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ExpressionSetterCreator = typeof(ReflectionExtensions).GetMethod("CreateSetterExpression", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo DelegateGetterWeakener = typeof(ReflectionExtensions).GetMethod("WeakenGetterDelegate", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo DelegateSetterWeakener = typeof(ReflectionExtensions).GetMethod("WeakenSetterDelegate", BindingFlags.Static | BindingFlags.NonPublic);

        private static GetterSetter GetOrCreateDelegates(MemberInfo memberInfo)
        {
            GetterSetter getterSetter;
            if (!DelegateCache.TryGetValue(memberInfo, out getterSetter))
            {
                getterSetter = new GetterSetter();

                var declaringType = memberInfo.DeclaringType;
                var memberType = memberInfo.GetMemberType();
                var parameterExpression = Expression.Parameter(declaringType);
                var isStatic = memberInfo.IsStatic();
                var memberExpression = Expression.MakeMemberAccess(isStatic ? null : parameterExpression, memberInfo);

                // getter
                if (memberInfo.CanRead())
                {
                    var typedLambda = ExpressionGetterCreator.MakeGenericMethod(declaringType, memberType);
                    var lambdaExpression = (LambdaExpression)typedLambda.Invoke(null, new object[] { memberExpression, new[] { parameterExpression } });
                    var func = lambdaExpression.Compile();
                    var weakenedDelegate = DelegateGetterWeakener.MakeGenericMethod(declaringType, memberType);
                    getterSetter.Getter = (Func<object, object>)weakenedDelegate.Invoke(null, new object[] { func });
                }

                // setter
                if (memberInfo.CanWrite())
                {
                    var parameterValueExpression = Expression.Parameter(memberType);
                    var memberSetExpression = Expression.Assign(memberExpression, parameterValueExpression);
                    var typedLambda = ExpressionSetterCreator.MakeGenericMethod(declaringType, memberType);
                    var lambdaExpression = (LambdaExpression)typedLambda.Invoke(null, new object[] { memberSetExpression, new[] { parameterExpression, parameterValueExpression } });
                    var funcSetter = lambdaExpression.Compile();
                    var weakenedDelegate = DelegateSetterWeakener.MakeGenericMethod(declaringType, memberType);
                    getterSetter.Setter = (Action<object, object>)weakenedDelegate.Invoke(null, new object[] { funcSetter });
                }

                DelegateCache.Add(memberInfo, getterSetter);
            }

            return getterSetter;
        }

        // ReSharper disable UnusedMember.Local

        private static LambdaExpression CreateGetterExpression<TSource, TResult>(Expression body, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<Func<TSource, TResult>>(body, parameters);
        }

        private static Func<object, object> WeakenGetterDelegate<TSource, TResult>(Func<TSource, TResult> func)
        {
            return o => func((TSource)o);
        }

        private static LambdaExpression CreateSetterExpression<TTarget, TMember>(Expression body, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<Action<TTarget, TMember>>(body, parameters);
        }

        private static Action<object, object> WeakenSetterDelegate<TTarget, TMember>(Action<TTarget, TMember> func)
        {
            return (t, v) => func((TTarget)t, (TMember)v);
        }
        // ReSharper restore UnusedMember.Local

        private class GetterSetter
        {
            public Func<object, object> Getter { private get; set; }
            public Action<object, object> Setter { private get; set; }

            public object InvokeGetter(MemberInfo member, object instance)
            {
                if (Getter == null)
                {
                    throw new ApplicationException(string.Format("Cannot get value for member [{0}]. Ensure that it is a field or a property with a getter.",
                      member.DeclaringType.FullName + "." + member.Name));
                }

                return Getter(instance);
            }

            public void InvokeSetter(MemberInfo member, object target, object value)
            {
                if (Setter == null)
                {
                    throw new ApplicationException(string.Format("Cannot set value on member [{0}]. Ensure that it is a property with a setter or a field that is not read-only.",
                      member.DeclaringType.FullName + "." + member.Name));
                }

                Setter(target, value);
            }
        }

        #endregion Helpers
    }
}
