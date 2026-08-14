using System.Collections.Generic;

namespace Celeste.Mod.XaphanHelper.Data
{
    public class LorebookData
    {
        public string EntryID;

        public int CategoryID;

        public string SubCategoryID;

        public string Name;

        public List<string> Text = new();

        public string Picture;

        public string Flag;

        public int Pages;

        public bool Code;

        public string DialogId;

        public LorebookData(string entryID, int categoryID, string picture, string flag, string subCategoryID = null, int pages = 1, bool code = false, string dialogID = null)
        {
            EntryID = entryID;
            CategoryID = categoryID;
            SubCategoryID = subCategoryID;
            Pages = pages;
            Code = code;
            DialogId = dialogID;

            string subStr = "";
            if (EntryID.Length > 1)
            {
                subStr = EntryID.Substring(1);
            }
            string convertedID = char.ToUpper(EntryID[0]) + subStr;

            Name = "LorebookEntry_" + convertedID + "_Name";
            Text.Add("LorebookEntry_" + convertedID + "_Text");
            if (Pages > 1)
            {
                for (int i = 2; i <= Pages; i++)
                {
                    Text.Add("LorebookEntry_" + convertedID + "_Text_" + i);
                }
            }
            Picture = picture;
            Flag = flag;
            Code = code;
        }
    }
}
