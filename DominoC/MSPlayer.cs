using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoC
{
    class MFPlayer
    {
        static public string PlayerName = "Андрей Борисов";

        static private List<MTable.SBone> Hand;
        static private List<MTable.SBone> SuitableForStep = new List<MTable.SBone>();
        static private List<MTable.SBone> SuitableForBlockingStep = new List<MTable.SBone>();
        static private List<MTable.SBone> Duplicates = new List<MTable.SBone>();
        static private List<ushort> OpponentMissingNums = new List<ushort>();

        private static byte numOfTaken;
        private static int lastShopCount;

        //=== Готовые функции =================
        // инициализация игрока
        static public void Initialize()
        {
            Hand = new List<MTable.SBone>();
        }

        // Вывод на экран
        static public void PrintAll()
        { MTable.PrintAll(Hand); }

        // дать количество доминушек
        static public int GetCount()
        { return Hand.Count; }

        //=== Функции для разработки =================
        // добавить доминушку в свою руку
        static public void AddItem(MTable.SBone sb)
        { Hand.Add(sb); }

        // Дать сумму очков на руке
        static public int GetScore()
        {
            int sum = 0;

            if (Hand.Count == 1 && Hand[0].First == 0 && Hand[0].Second == 0) return 25;

            else
            {
                foreach (MTable.SBone sb in Hand)
                    sum += sb.First + sb.Second;

                return sum;
            }
        }


        // Сделать ход
        static public bool MakeStep(out MTable.SBone sb, out bool End)
        {

            int myLastCount = GetCount();
            List<MTable.SBone> Game = MTable.GetGameCollection();

            SuitableForStep = new List<MTable.SBone>();
            SuitableForBlockingStep = new List<MTable.SBone>();
            Duplicates = new List<MTable.SBone>();

            ushort LeftEdge = Game[0].First;
            ushort RightEdge = Game[Game.Count - 1].Second;

            // Выбираем подходящие для хода доминошки
            while (SuitableForStep.Count == 0)
            {
                foreach (MTable.SBone bone in Hand)
                {
                    if (bone.First == Game[0].First || bone.Second == Game[0].First || bone.First == Game[Game.Count - 1].Second || bone.Second == Game[Game.Count - 1].Second)
                        SuitableForStep.Add(bone);
                }
                if (SuitableForStep.Count == 0)
                {
                    if (MTable.GetShopCount() > 0)
                    {
                        MTable.SBone newsb;
                        MTable.GetFromShop(out newsb);
                        Hand.Add(newsb);
                    }
                    else
                    {
                        sb = Hand[0];
                        End = true;
                        return false;
                    }
                }
            }

            // Если взяли, но у меня не увеличилось => оставить числа, если берет первый раз
            // После второго раза нет гарантии, что этих чисел больше нет у оппонента
            // При обратной ситуации последние два добавленные числа — неактуальны 
            if (MTable.GetShopCount() < lastShopCount && myLastCount >= GetCount())
            {
                numOfTaken++;
                if (numOfTaken == 2)
                {
                    if (OpponentMissingNums.Count > 0) OpponentMissingNums.RemoveAt(0);
                    if (OpponentMissingNums.Count > 0) OpponentMissingNums.RemoveAt(0);
                    numOfTaken = 0;
                }
            }
            else
            {
                if (OpponentMissingNums.Count > 0) OpponentMissingNums.RemoveAt(OpponentMissingNums.Count - 1);
                if (OpponentMissingNums.Count > 0) OpponentMissingNums.RemoveAt(OpponentMissingNums.Count - 1);
                numOfTaken = 0;
            }


            // Ищем дубли в подходящих для хода
            foreach (MTable.SBone bone in SuitableForStep)
            {
                if (bone.First == bone.Second)
                {
                    Duplicates.Add(bone);
                }
            }

            // Если подходит одна доминошка
            if (SuitableForStep.Count == 1)
            {
                sb = SuitableForStep[0];
                End = ChooseSide(sb, Game);
                Hand.Remove(sb);
            }

            // Если подходит несколько доминошек
            else
            {
                // Если есть дубликаты
                if (Duplicates.Count > 0)
                {
                    if (IsThereNullDupl(Duplicates, out MTable.SBone nullDupl)) sb = nullDupl;
                    else sb = Max(Duplicates);                         // Выбрать дубликат с большей суммой

                    End = ChooseSide(sb, Game);
                    Hand.Remove(sb);
                }

                // Если дубликатов нет
                else
                {
                    foreach (MTable.SBone bone in SuitableForStep)
                    {
                        if (OpponentMissingNums.Contains(bone.First) || OpponentMissingNums.Contains(bone.Second))
                            SuitableForBlockingStep.Add(bone);
                    }

                    if (SuitableForBlockingStep.Count > 0)
                    {
                        sb = Max(SuitableForBlockingStep);
                        End = ChooseSide(sb, Game);
                        Hand.Remove(sb);
                    }

                    else
                    {
                        // Выбрать, что выгоднее:
                        // меньшее число снаружи
                        // большая сумма
                        if (Math.Abs(Sum(Max(SuitableForStep)) - Sum(FindSmallestOut(SuitableForStep, Game))) <= 2)
                        {
                            sb = FindSmallestOut(SuitableForStep, Game);
                            End = ChooseSide(sb, Game);
                            Hand.Remove(sb);
                        }

                        else
                        {
                            sb = Max(SuitableForStep);
                            End = ChooseSide(sb, Game);
                            Hand.Remove(sb);
                        }
                    }


                }
            }

            lastShopCount = MTable.GetShopCount();
            OpponentMissingNums.Add(LeftEdge);
            OpponentMissingNums.Add(RightEdge);
            return true;
        }



        public static bool ChooseSide(MTable.SBone sb, List<MTable.SBone> lGame)
        {

            ushort LeftEdge = lGame[0].First;
            ushort RightEdge = lGame[lGame.Count - 1].Second;

            // Если доминошку можно поставить с двух сторон
            if ((sb.First == LeftEdge || sb.Second == LeftEdge) && (sb.First == RightEdge || sb.Second == RightEdge))
            {
                // Если только одно число на доминошек отсутствует в руке оппонента
                if (OpponentMissingNums.Contains(sb.First) ^ OpponentMissingNums.Contains(sb.Second))
                {
                    if (sb.First == LeftEdge || sb.Second == LeftEdge) return false;
                    else return true;
                }

                // Если слева число из списка отсутствующих
                else if (OpponentMissingNums.Contains(LeftEdge))
                {
                    // поставить направо
                    return true;
                }

                else if (OpponentMissingNums.Contains(RightEdge))
                {
                    return false;
                }
                // Если два или ноль чисел на доминошке отсутствует в руке оппонента
                else
                {
                    // Ставим туда, где больше
                    return LeftEdge < RightEdge;
                }
            }

            // Если можно поставить только слева
            else if (sb.First == LeftEdge || sb.Second == LeftEdge)
            {
                // Ставим слева
                return false;
            }

            // Если можно поставить только справа
            else if (sb.First == RightEdge || sb.Second == RightEdge)
            {
                // Ставим справа
                return true;
            }

            else return LeftEdge < RightEdge;
        }


        public static MTable.SBone FindSmallestOut(List<MTable.SBone> SBones, List<MTable.SBone> Game)
        {
            List<MTable.SBone> Suits = new List<MTable.SBone>();
            ushort sbmin = 6;
            ushort LeftEdge = Game[0].First;
            ushort RightEdge = Game[Game.Count - 1].Second;
            foreach (MTable.SBone sb in SBones)
            {
                if ((sb.First == LeftEdge || sb.First == RightEdge) && sb.Second <= sbmin)
                {
                    Suits.Add(sb);
                }
                else if ((sb.Second == LeftEdge || sb.Second == RightEdge) && sb.First <= sbmin)
                {
                    Suits.Add(sb);
                }
            }

            return Max(Suits);
        }

        public static bool IsThereNullDupl(List<MTable.SBone> Duplicates, out MTable.SBone nullDupl)
        {
            foreach (MTable.SBone sb in Duplicates)
            {
                if (sb.First == 0 && sb.Second == 0)
                {
                    nullDupl = sb;
                    return true;
                }
            }

            nullDupl = Duplicates[0];
            return false;
        }

        // Возвращает сумму чисел на доминошке
        static public int Sum(MTable.SBone sb)
        { return sb.First + sb.Second; }

        // Возвращает доминошку из списка с большей суммой
        static public MTable.SBone Max(List<MTable.SBone> list)
        {
            int msum = 0;
            MTable.SBone max = list[0];
            foreach (MTable.SBone sb in list)
            {
                if (Sum(sb) > msum)
                {
                    msum = Sum(sb);
                    max = sb;
                }
            }
            return max;
        }

    }
}
