using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Reactive.Bindings;
using Prism.Regions;
using Unity;

using FF4EncountViewer.Models;
using System.Collections.ObjectModel;

namespace FF4EncountViewer.ViewModels
{
    public class MonsterEncountViewModel : BindableBase
    {
        [Dependency]
        public IModel Model { get; set; }

        public ReactiveProperty<int> SelectedPlaceItem { get; } = new ReactiveProperty<int>(0);
        public ReactiveProperty<int> SelectedEncountItem { get; } = new ReactiveProperty<int>(-1);
        public ReactiveProperty<int> CurrentEncount { get; } = new ReactiveProperty<int>();

        public ReactiveProperty<string> AvilableTable { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> EncountLog { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> EncountForecast { get; } = new ReactiveProperty<string>();

        public ReactiveCollection<string> PlaceListItems { get; set; } = new ReactiveCollection<string>();
        public ReactiveCollection<Encount> NextAvailableEncounts { get; set; } = new ReactiveCollection<Encount>();
        //public ObservableCollection<Encount> NextAvailableEncounts { get; set; } = new ObservableCollection<Encount>();
        
        //public List<Encount> NextAvailableEncounts { get; set; } = new List<Encount>();
        public ReactiveCommand<object> LoadedCommand { get; set; }
        public ReactiveCommand<object> NextEncountCommand { get; set; }
        public ReactiveCommand<object> UndoEncountCommand { get; set; }

        public class Encount
        {
            public int ID { get; set; }
            public string EncountName { get; set; }
        }

        public MonsterEncountViewModel()
        {

            LoadedCommand = new ReactiveCommand()
                .WithSubscribe( ()=>
                {
                    Initialize();
                });

            NextEncountCommand = new ReactiveCommand()
                .WithSubscribe(()=>
                {
                    NextEncount();
                });

            UndoEncountCommand = new ReactiveCommand()
                .WithSubscribe(()=>
                {
                    UndoEncount();
                });


            SelectedPlaceItem.Subscribe(_=>SetEncounts());

            //NextAvailableEncounts.Add(new Encount());


        }

        private void Initialize()
        {
            Model.InitializeEncountTable();

            Model model = Model as Model;
            model.encountTable.lstFieldName.ForEach(x => PlaceListItems.Add(x));
            model.Reset();
            SelectedPlaceItem.Value = 1;
            SetEncounts();
        }

        private bool isOperatingNextEncount = false;

        private void NextEncount()
        {
            if (isOperatingNextEncount)
                return;
            isOperatingNextEncount = true;
            Model model = Model as Model;
            Encount e = NextAvailableEncounts[SelectedEncountItem.Value];
            model.Next(e.ID);
            SetEncounts();
            isOperatingNextEncount = false;
        }

        private class EncountPattern
        {
            public List<int> indices = new List<int>();
            public string partyPattern;
        }

        private void UndoEncount()
        {
            if (CurrentEncount.Value <= 0)
                return;
            CurrentEncount.Value--;
            Model model = Model as Model;
            model.UndoEncount();
            SetEncounts();
        }

        private void SetEncounts()
        {
            Model model = Model as Model;
            if (model == null)
                return;

            bool[] currentAvailables = model.CurrentAvailableMonsterPartyIndeces();

            AvilableTable.Value = string.Join(",",model.CurrentAvailableTableIndex());

            NextAvailableEncounts.Clear();
            foreach(var i in Enumerable.Range(0, 8))
            {
                if (currentAvailables[i])
                {
                    Encount newEncount = new Encount();
                    newEncount.ID = i;
                    newEncount.EncountName = $"{model.encountTable.pattern[SelectedPlaceItem.Value].party[i]}";
                    NextAvailableEncounts.Add(newEncount);
                }
            }

            EncountLog.Value = "";
            foreach (var i in Enumerable.Range(0, model.currentEncountIndex))
            {
                var tableIndices = model.CurrentAvailableTableIndex();
                int partyIndex = model.encountTable.table[i, tableIndices[0]];
                string partyPattern = model.encountTable.pattern[SelectedPlaceItem.Value].party[partyIndex];
                EncountLog.Value += $"{i} : {partyPattern}\n";
            }

            //update forcast
            EncountForecast.Value = "";
            foreach (var i in Enumerable.Range(0,32))
            {
                int encountCounter = model.currentEncountIndex + i;
                EncountForecast.Value += $"{encountCounter}\n";
                var tableIndices = model.CurrentAvailableTableIndex();

                List<EncountPattern> lstEncountPatterns = new List<EncountPattern>();
                foreach(var tableIndex in tableIndices)
                {
                    int partyIndex = model.encountTable.table[encountCounter, tableIndex];
                    string partyPattern = model.encountTable.pattern[SelectedPlaceItem.Value].party[partyIndex];

                    var array = (from x in lstEncountPatterns where x.partyPattern == partyPattern select x).ToArray();
                    if (array.Length == 0)
                    {
                        EncountPattern ep = new EncountPattern();
                        ep.indices.Add(tableIndex);
                        ep.partyPattern = partyPattern;
                        lstEncountPatterns.Add(ep);
                    }
                    else
                    {
                        array[0].indices.Add(tableIndex);
                    }
                }
                foreach (var pattern in lstEncountPatterns)
                {
                    EncountForecast.Value += $"\t{string.Join(",", pattern.indices)} : {pattern.partyPattern}\n";
                }
                EncountForecast.Value += "\n";
            }

            CurrentEncount.Value = model.currentEncountIndex;
        }
    }
}
