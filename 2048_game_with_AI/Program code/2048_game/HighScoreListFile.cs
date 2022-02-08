﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace _2048_game
{
    public class HighScoreListFile
    {
        private string _filename = "HighScoreList.json";

        public IEnumerable<HighScoreListModel> ReadFile()
        {
            if (File.Exists(_filename))
            {
                var jsonString = File.ReadAllText(_filename);
                var data = JsonSerializer.Deserialize<IEnumerable<HighScoreListModel>>(jsonString);
                return data;
            }
            return new List<HighScoreListModel>();
        }

        public void WriteFile(IEnumerable<HighScoreListModel> data)
        {
            var jsonString = JsonSerializer.Serialize(data);
            File.WriteAllText(_filename, jsonString);
        }
    }
}
