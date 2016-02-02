using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Todo
{
    [Serializable]
    public class TodoData : ScriptableObject
    {
        public List<TodoEntry> Entries = new List<TodoEntry>();

        public List<string> TagsList = new List<string>()
        {
            "TODO",
            "BUG"
        };

        public int EntriesCount
        {
            get { return Entries.Count; }
        }

        public int TagsCount
        {
            get { return TagsList.Count; }
        }

        public int GetCountByTag(int tag)
        {
            return tag != -1 ? Entries.Count(e => e.Tag == TagsList[tag]) : EntriesCount;
        }

        public TodoEntry GetEntryAt(int index)
        {
            return Entries[index];
        }

        public void AddTag(string tag)
        {
            if (TagsList.Contains(tag) || string.IsNullOrEmpty(tag))
                return;
            TagsList.Add(tag);
        }

        public void RemoveTag(int index)
        {
            if (TagsList.Count >= (index + 1))
                TagsList.RemoveAt(index);
        }
    }

}