using System.Collections.Generic;

namespace HzNS.Cmdr
{
    public class Slot
    {
        public Slot()
        {
            Children = new Slots();
            Values = new SlotEntries();
        }

        // ReSharper disable once CollectionNeverUpdated.Global
        public Slots Children { get; }
        public SlotEntries Values { get; }

        public static Slot Empty { get; } = new Slot();
    }

    public class SlotEntries : Dictionary<string, object?>
    {
    }

    public class Slots : Dictionary<string, Slot>
    {
    }
    
    public class SortedSlots : SortedDictionary<string, Slot>
    {
    }
}