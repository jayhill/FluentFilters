using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using DataAccess;
using FluentFilters.Fluent;

namespace FluentFilters
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            var datatable = DataTable.New.ReadCsv("baseball_teams.csv");
            Data = datatable.RowsAs<TeamSeason>().ToList();
            CollectionView = (ListCollectionView) CollectionViewSource.GetDefaultView(Data);
            FilterBar = new FilterBar<TeamSeason>("Filter Bar", Data, GetFilters().ToArray());
        }

        public IList<TeamSeason> Data { get; set; }

        public FilterBar<TeamSeason> FilterBar { get; set; }

        public ListCollectionView CollectionView { get; set; }

        private IEnumerable<FilterBase<TeamSeason>> GetFilters()
        {
            // Play around -- add your own filters here

            yield return Filter.For<TeamSeason>("Name/City/Park")
                .SearchOn(x => x.Name).Contains
                .SearchOn(x => x.Park).StartsWith;

            yield return Filter.For<TeamSeason>("First Place Seasons")
                .Checkbox.PassesIf(x => x.Rank == 1);

            yield return Filter.For<TeamSeason>("Home Runs")
                .Select.RangeOn(x => x.HomeRuns)
                .AddRange("<100", 0, 100)
                .AddRange("101-200", 101, 200)
                .AddRange("201+", 201, int.MaxValue);

            yield return Filter.For<TeamSeason>("League")
                .Select.On(x => x.League)
                .WithDataFrom(Data)
                .OrderAsc;
        }
    }
}