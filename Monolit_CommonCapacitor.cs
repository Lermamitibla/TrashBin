using System;
using System.Linq;
using System.Windows.Forms;

namespace AutoProt.Plants
{
    class Monolit_CommonCapacitor : Item
    {
       protected string type = String.Empty;
        protected string contPads = String.Empty;
        protected string TKE = String.Empty;
        protected double capacity = 0d;
        protected double voltage = 0d;
        protected string TCE = String.Empty;
        protected double tolerance = 0d;
        protected string caseCode = String.Empty;
        protected string capacityGrade = String.Empty;
        protected string capacityString = String.Empty;
        //RealType содержит только тип (без корпуса), для формирования партномера
        protected string realType = String.Empty;
        protected string toleranceForPN = String.Empty;

        public Monolit_CommonCapacitor(string partnumber, int quantity, string priemka, int stringnumber) : base(partnumber, quantity, priemka, stringnumber)
        {
            _leadTime = "70-90 дней";
            _manufacturer = "Завод Монолит";
            _supplier = "Спецэлектронкомплект";
            _itemRecognized = true;
            Console.WriteLine("Был вызван конструктор МонолитОбщий для " + base.partnumber);
        }
        protected int CheckVoltage(string[] acceptedVoltageStrings)
        {
            //Возвращает -1, если произошла ошибка при разборе, 0 если нет значения, 1 если значение успешно найдено
            bool haveVoltage = false;

            foreach (string acceptedString in acceptedVoltageStrings)
            {
                if (partnumber.Contains(acceptedString))
                {
                    haveVoltage = true;
                    if (acceptedString.Contains("КВ"))
                    {
                        bool findVoltage = Double.TryParse(acceptedString.Substring(0, acceptedString.IndexOf('К')), out double result);
                        if (findVoltage)
                        {
                            voltage = result * 1000;
                            partnumber.Remove(partnumber.IndexOf(acceptedString), acceptedString.Length);
                        }
                        else
                        {
                            _comments.Insert(0, "Ошибка в разборе напряжения; ");
                            _recognizeSucceed = false;
                            return -1;
                        }
                    }
                    if (acceptedString.Contains("В") && !acceptedString.Contains("КВ"))
                    {
                        bool findVoltage = Double.TryParse(acceptedString.Substring(0, acceptedString.IndexOf('В')), out double result);
                        if (findVoltage)
                        {
                            voltage = result;
                            partnumber.Remove(partnumber.IndexOf(acceptedString), acceptedString.Length);
                        }
                        else
                        {
                            _comments.Insert(0, "Ошибка в разборе напряжения; ");
                            _recognizeSucceed = false;
                            return -1;
                        }
                    }
                }
            }
            if (!haveVoltage)
            {
                if (!type.Contains("К10-17")) _comments.Insert(0, "Не удалось распознать напряжение конденсатора.");
                return 0;
            }
            else return 1;
        }
        protected void CheckCapacity()
        {
            char[] acceptedChars = "0123456789,".ToCharArray();

            if (partnumber.Contains('Ф'))
            {
                int indexOfF = partnumber.IndexOf('Ф');
                // возможный крайний символ, содержащий цифру
                int lastIndex = indexOfF - 2;
                int indexOfDash = lastIndex;
                char[] partnumberChars = partnumber.ToString().ToCharArray();
                // ищем разделительное тире левее Ф, если оно вообще есть
                if (partnumber.Substring(0, indexOfF).Contains('-'))
                {
                    for (int a = 0; a < 10; a++)
                    {
                        if (partnumberChars[indexOfDash] == '-') break;
                        else indexOfDash--;
                    }
                }
                else
                {
                    _comments.Insert(0, "Не удается распознать емкость, поставьте, пожалуйста, тире перед емкостью, согласно правилам составления ариткула, описаным в документации Монолита; ");
                    _recognizeSucceed = false;
                    return;
                }

                //нашли первое тире, теперь начнем формировать номинал. Дойдем до первой буквы и парсим подстроку
                int firstLetter = indexOfDash + 1;

                bool cicleContinue = true;
                while (cicleContinue)
                {
                    bool IsAvailable = false;
                    foreach (char Available in acceptedChars)
                    {
                        if (partnumberChars[firstLetter] == Available)
                        {
                            IsAvailable = true;
                        }
                    }
                    if (!IsAvailable) cicleContinue = false;
                    else firstLetter++;

                }

                string nominal = partnumber.Substring(indexOfDash + 1, firstLetter - indexOfDash - 1);

                partnumber.Remove(partnumber.IndexOf(nominal), nominal.Length);

                bool findNominal = Double.TryParse(nominal, out double result);
                if (findNominal) capacity = result;
                else
                {
                    Console.WriteLine("Error with item: ");
                    MessageBox.Show("Error with parsing capacity");
                }
            }
            else
            {
                _comments.Insert(0, "Не указан номинал...  оО; ");
                _recognizeSucceed = false;
                return;
            }

            if (partnumber.Contains("ПФ")) capacityGrade = "пФ";
            if (partnumber.Contains("МКФ")) capacityGrade = "мкФ";

            capacityString = capacity.ToString().Replace(',', '.');
        }

        protected void CheckTolerance()
        {
            string[] toleranceVariants = { "0,25ПФ", "0,5ПФ", "1ПФ", "0,25%", "0,5%", "1%", "2%", "5%", "10%", "50", "80", "20%" };

            bool findTol = false;
            foreach (string variant in toleranceVariants)
            {
                if (partnumber.Contains(variant))
                {
                    partnumber.Remove(partnumber.IndexOf(variant), variant.Length);
                    if (variant == "0,25ПФ")
                    {
                        tolerance = 0.25;
                        toleranceForPN = "±0,25пФ";
                    }
                    if (variant == "0,5ПФ")
                    {
                        toleranceForPN = "±0,5пФ";
                        tolerance = 0.5;
                    }
                    if (variant == "1ПФ")
                    {
                        toleranceForPN = "±1пФ";
                        tolerance = 1d;
                    }
                    if (variant == "0,25%")
                    {
                        tolerance = 0.25;
                        toleranceForPN = "±0,25пФ";
                    }
                    if (variant == "0,5%")
                    {
                        toleranceForPN = "±0,5пФ";
                        tolerance = 0.5;
                    }
                    if (variant == "1%")
                    {
                        toleranceForPN = "±1%";
                        tolerance = 1d;
                    }
                    if (variant == "2%")
                    {
                        toleranceForPN = "±2%";
                        tolerance = 2d;
                    }
                    if (variant == "5%")
                    {
                        toleranceForPN = "±5%";
                        tolerance = 5d;
                    }
                    if (variant == "10%")
                    {
                        toleranceForPN = "±10%";
                        tolerance = 10d;
                    }
                    if (variant == "50")
                    {
                        toleranceForPN = "+50/-20%";
                        tolerance = 50d;
                    }
                    if (variant == "80")
                    {
                        toleranceForPN = "+80/-20%";
                        tolerance = 80d;
                    }
                    if (variant == "20%")
                    {
                        toleranceForPN = "±20%";
                        tolerance = 20d;
                    }
                    findTol = true;
                    partnumber.Remove(0, variant.Length);
                }
            }
            if (!findTol)
            {
                _comments.Insert(0, "Не удалось распознать допуск; ");
                _recognizeSucceed = false;
                return;
            }
        }

        protected void CheckTCE(string[] TCEVariants)
        {

            bool findTCE = false;
            foreach (string variant in TCEVariants)
            {
                if (partnumber.Contains(variant))
                {
                    TCE = variant;
                    findTCE = true;
                    if (TCE == "МПО") TCE = "МП0";
                    partnumber.Remove(partnumber.IndexOf(variant), variant.Length);
                    return;
                }
            }
            if (!findTCE)
            {
                _comments.Insert(0, "Не удалось распознать ТСЕ (Н30, Н20, Н90, МП0, М47 или М1500; ");
                _recognizeSucceed = false;
                return;
            }
        }

        protected void CheckNikelPads()
        {
           // Console.WriteLine("Partnumber in checking N: " + Partnumber);
            if (partnumber.Contains('N') || partnumber.Contains("ЛУЖ")) contPads = "N";
            else contPads = "";
        }
    }
}
