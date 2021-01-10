using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF4EncountViewer.Models
{
    public class EncountTable
    {
        public readonly int[,] table = new int[254, 128];
        public List<MonsterPartyPattern> pattern { get; private set; }
        public List<string> lstFieldName { get; private set; }
        public EncountTable()
        {
            var lstFF4EncountTable = Model.LoadCSV("data/FF4.csv");
            for (int j = 0; j < 128; j++)
            {
                for (int i = 0; i < 254; i++)
                {
                    table[i, j] = int.Parse(lstFF4EncountTable[j][i + 2]) - 1;
                }
            }

            var csvFieldIDToNameTable = Model.LoadCSV("data/ff4monenc.txt", ':',false);
            pattern = MonsterEncountParse(csvFieldIDToNameTable);

            lstFieldName = new List<string>();
            var lstFF4FieldToName = Model.LoadCSV("data/FF4FieldIDToName.csv");
            foreach (var line in lstFF4FieldToName)
            {
                if (line[0][0] == '#')
                    continue;

                lstFieldName.Add(line[1]);
            }
        }

        public class MonsterPartyPattern
        {
            public int partyPatternIndex;
            public string[] party = new string[8];

            public MonsterPartyPattern(int index)
            {
                partyPatternIndex = index;
            }
        }

        private List<MonsterPartyPattern> MonsterEncountParse(List<List<string>> csvFieldIDToNameTable)
        {
            List<MonsterPartyPattern> lstMonsterPartyPattern = new List<MonsterPartyPattern>();
            MonsterPartyPattern currentMonsterPartyPattern = null;
            char[] del = { '(', ')' };
            {
                // 末尾まで繰り返す
                foreach (var line in csvFieldIDToNameTable)
                {
                    //ここで解析
                    /*
#[No.000(00)]
01: 000(000):  ゴブリン          3
02: 001(001):  フロータイボール  2
                     */
                    if (line[0].Contains("#[No."))
                    {
                        var parts = line[0].Split(del);
                        int indexPlace = Convert.ToInt32(parts[1], 16);
                        currentMonsterPartyPattern = new MonsterPartyPattern(indexPlace);
                        lstMonsterPartyPattern.Add(currentMonsterPartyPattern);

                    }
                    else if (line[0][0] == '#')
                        continue;
                    else
                    {
                        int monsterPartyIndex = int.Parse(line[0]) - 1;
                        string monsterParty = line[2];
                        if (currentMonsterPartyPattern != null)
                            currentMonsterPartyPattern.party[monsterPartyIndex] = monsterParty;
                    }
                }
            }

            return lstMonsterPartyPattern;

        }
    }

    class Model : IModel
    {
        private List<List<int>> availablesLog = new List<List<int>>();
        public EncountTable encountTable { get; private set; }
        public int currentEncountIndex = -1;
        public void InitializeEncountTable()
        {
            encountTable = new EncountTable();
            Reset();
        }

        public void Reset()
        {
            availablesLog.Clear();
            List<int> lstFirst = new List<int>();
            foreach (var i in Enumerable.Range(0, 128))
                lstFirst.Add(i);
            availablesLog.Add(lstFirst);
            currentEncountIndex = 0;
        }

        public void Next(int index)
        {
            var nextList = (from x in availablesLog[currentEncountIndex]
             where encountTable.table[currentEncountIndex % 254, x] == index
             select x).ToList();

            availablesLog.Add(nextList);
            currentEncountIndex++;
        }

        public void UndoEncount()
        {
            availablesLog.RemoveAt(currentEncountIndex);
            currentEncountIndex--;
        }


        public int[] CurrentAvailableTableIndex() => availablesLog[currentEncountIndex].ToArray();

        public bool[] CurrentAvailableMonsterPartyIndeces()
        {
            bool[] result = new bool[8];
            foreach (var i in Enumerable.Range(0, 8))
                result[i] = false;
            foreach(var x in availablesLog[currentEncountIndex])
            {
                int indexEncount = encountTable.table[currentEncountIndex,x];
                result[indexEncount] = true;
            }
            return result;
        }

        
        public static List<List<string>> LoadCSV(string path,char sep = ',',bool skipSharp = true)
        {
            List<List<string>> retval = new List<List<string>>();
            StreamReader sr = new StreamReader(path);
            {
                // 末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    // CSVファイルの一行を読み込む
                    string line = sr.ReadLine();
                    if (skipSharp && line[0] == '#')
                        continue;
                    // 読み込んだ一行をカンマ毎に分けて配列に格納する
                    string[] values = line.Split(sep);

                    // 配列からリストに格納する
                    List<string> lists = new List<string>();
                    lists.AddRange(values);

                    retval.Add(lists);
                }
            }
            return retval;
        }
    }
}
