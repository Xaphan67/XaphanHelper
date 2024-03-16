namespace Celeste.Mod.XaphanHelper.Data
{
    public class LorebookData
    {
        public string EntryID;

        public int CategoryID;

        public string SubCategoryID;

        public string Name;

        public string Text;

        public string Picture;

        public string Flag;

        public LorebookData(string entryID, int categoryID, string picture, string flag, string subCategoryID = null)
        {
            EntryID = entryID;
            CategoryID = categoryID;
            SubCategoryID = subCategoryID;

            string subStr = "";
            if (EntryID.Length > 1)
            {
                subStr = EntryID.Substring(1);
            }
            string convertedID = char.ToUpper(EntryID[0]) + subStr;

            Name = "LorebookEntry_" + convertedID + "_Name";
            Text = "LorebookEntry_" + convertedID + "_Text";
            Picture = picture;
            Flag = flag;
        }
    }
}
