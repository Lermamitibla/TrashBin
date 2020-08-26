using System;
using System.Collections.Generic;
using AutoProt.Plants;

namespace AutoProt
{
    class Search
    {
        //List<Item> items = new List<Item>();
      //  List<SimleItem> items = new List<SimleItem>();
       // public SimleItem completedItem;

        internal SimleItem CheckManufacturer (string Partnumber,string Quantity, string Priemka, int StringNumber)
        {
            //Тут приводим к общему виду партномер(капс, пробелы, лишние знаки, запятые и точки)
            Partnumber = Partnumber.ToUpper();
            Partnumber = Partnumber.Replace('.', ',');
            int CheckRedundant = Partnumber.IndexOfAny(Const.REDUNDANT_CHARS);
            //Пока есть в строке лишние символы - удаляем их.
            while (CheckRedundant != -1)
            {
                Partnumber = Partnumber.Remove(CheckRedundant, 1);
                CheckRedundant = Partnumber.IndexOfAny(Const.REDUNDANT_CHARS);
            }


            int qty = 0;
            bool TryParseQTY = Int32.TryParse(Quantity, out int result);
            if (TryParseQTY) qty = result;

            bool isImport = true;
            char[] partnumberChars = Partnumber.ToCharArray();
            foreach (char a in Const.RUSSIAN_ALPHABETE)
            {
                foreach (char b in partnumberChars)
                {
                    if (a == b) isImport = false;
                }
            }

            List<String> stockList = new List<string>();
            stockList.Add("Digi-key");

            bool haveFoundInStock = true;

            if (isImport)
            {
                Console.WriteLine("Создаю фабрику импорта");
                ImportItemFabric fabric = new ImportItemFabric(stockList, Partnumber, qty, Priemka, StringNumber);

                Item item = fabric.GetBestImportItem();

                if (item != null)
                {
                    item.ItemRecognized = true;
                    return Const.Adapter(item);
                }
                else haveFoundInStock = false;
                
            }









            // bool FoundSpecial = false;
            if (Partnumber.Contains("Р1-12"))
            {
                ErkonR1_12VP item = new ErkonR1_12VP(Partnumber, qty, Priemka, StringNumber);
                return Const.Adapter(item);
                //FoundSpecial = true;
            }
            if (Partnumber.Contains("2РМДТ"))
            {
                Elecon2RMD_class item = new Elecon2RMD_class(Partnumber, qty, Priemka, StringNumber);
                return Const.Adapter(item);
                // FoundSpecial = true;  
            }
            if (Partnumber.Contains("2РМТ"))
            {
                Elecon2RMT_class item = new Elecon2RMT_class(Partnumber, qty, Priemka, StringNumber);
                return Const.Adapter(item);
                // FoundSpecial = true;
            }
            if (Partnumber.Contains("2РТТ"))
            {
                Elecon2TT_class item = new Elecon2TT_class(Partnumber, qty, Priemka, StringNumber);
                return Const.Adapter(item);
                // FoundSpecial = true;
            }
            if (Partnumber.Contains("С2-33Н"))
            {
                ErkonC2_33H_VP_Item item = new ErkonC2_33H_VP_Item(Partnumber, qty, Priemka, StringNumber);
                return Const.Adapter(item);
                // FoundSpecial = true;
            }
            if (Partnumber.Contains("К53-68"))
            {
                ElecondK53_68_class item = new ElecondK53_68_class(Partnumber, qty, Priemka, StringNumber);
                return Const.Adapter(item);
                // FoundSpecial = true;
            }
            if (Partnumber.Contains("К10-47М") || Partnumber.Contains("К10-84"))
            {
                Monolit_K10_47m_84class item = new Monolit_K10_47m_84class(Partnumber, qty, Priemka, StringNumber);
                return Const.Adapter(item);
                // FoundSpecial = true;
            }
            if (Partnumber.Contains("К10-17"))
            {
                Monolit_K10_17_class item = new Monolit_K10_17_class(Partnumber, qty, Priemka, StringNumber);
                return Const.Adapter(item);
                // FoundSpecial = true;
            }
            if (Partnumber.Contains("К10-43"))
            {
                Monolit_K10_43_class item = new Monolit_K10_43_class(Partnumber, qty, Priemka, StringNumber);
                return Const.Adapter(item);
                // FoundSpecial = true;
            }
            if (Partnumber.Contains("К10-50В"))
            {
                Monolit_K10_50class item = new Monolit_K10_50class(Partnumber, qty, Priemka, StringNumber);
                return Const.Adapter(item);
                // FoundSpecial = true;
                
            }
            if (Partnumber.Contains("КМ-4") || Partnumber.Contains("КМ-5"))
            {
                Monolit_KM4_5_class item = new Monolit_KM4_5_class(Partnumber, qty, Priemka, StringNumber);
                return Const.Adapter(item);
                // FoundSpecial = true;

            }



            SimleItem nothing = new SimleItem(StringNumber);
            if (!haveFoundInStock)
            {
                nothing.comments.Insert(0, "Нет на стоках; ");
            }
            return nothing;

           // if (!FoundSpecial) items.Add(CommonItem);
        }
        //internal List<SimleItem> ReturnItemsArray()
        //{
        //    return items;
        //}

    }
}
