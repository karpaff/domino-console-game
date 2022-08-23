using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoC
{
    class MTable
    {
        // number of bones in the begining
        public const int conStartCount = 7;
        // state of the game
        public enum EFinish {Play=0, First, Second, Lockdown}; 

        // One bone
        public struct SBone
        {
            public ushort First;
            public ushort Second;

            public void Exchange()
            {
                ushort shrTemp = First;
                First = Second;
                Second= shrTemp;
            }
        }

        static private List<SBone> lBoneyard;
        // Current game layout
        static private List<SBone> lGame;
        // Number of the step
        static private int intGameStep =1;
        // Number of bones the player took last time
        static private int intLastTaken, intTaken;
        // Random nums generator
        static private Random rnd;


        //***********************************************************************
        // Game initialization
        //***********************************************************************
        static private void Initialize()
        {
            SBone sb;

            rnd = new Random();
            // Clear collection
            lBoneyard = new List<SBone>();
            lGame = new List<SBone>();
        
            // Filling boneyard
            for (ushort shrC = 0; shrC<=6; shrC++)
                for (ushort shrB = shrC; shrB<=6; shrB++)
                {
                    sb.First = shrC;
                    sb.Second = shrB;
                    lBoneyard.Add(sb);
                }

            // Player initialization
            MFPlayer.Initialize();
            MSPlayer.Initialize();
        }

        //***********************************************************************
        // Gets a random bone (sb) from the boneyard
        // Returns FALSE, if boneyard is empty
        //***********************************************************************
        static public bool GetFromShop(out SBone sb)
        {
            int intN;
            sb.First = 7; sb.Second = 7;
        
            if (lBoneyard.Count == 0) return false;

            // Nums of bones taken for the current move
            intTaken += 1;
            // Defines random bone from the boneyard
            intN = rnd.Next(lBoneyard.Count -1);
            sb = lBoneyard[intN];
            // Deletes it from the boneyard 
            lBoneyard.RemoveAt(intN);
            Console.WriteLine("Taken from the boneyard: " + sb.First + ":" + sb.Second + " ");
            return true;
        }

        //***********************************************************************
        // Returns number of bones left in the boneyard
        //***********************************************************************
        static public int GetShopCount()
        { 
            return lBoneyard.Count;
        }

        //***********************************************************************
        // Returns numbers of the bones taken for the current move
        //***********************************************************************
        static public int GetTaken()
        {return intLastTaken;}

        //***********************************************************************
        // Returns current game layout
        //***********************************************************************
        static public List<SBone> GetGameCollection()
        { return lGame.ToList();  }

        //***********************************************************************
        // Handis out dominoes to both players at the beginning of the game
        //***********************************************************************
        static public void GetHands()
        {
            SBone sb;
            for(int intC = 0 ; intC < conStartCount; intC++)
            {
                if (GetFromShop(out sb)) MFPlayer.AddItem(sb);
                intTaken = 0;
                if (GetFromShop(out sb)) MSPlayer.AddItem(sb);
                intTaken = 0;
            }
       }

        //***********************************************************************
        // Prints all elements of the collection
        //***********************************************************************
        static public void PrintAll(List<SBone> lItems)
        {
            foreach(SBone sb in lItems)
                Console.Write(sb.First + ":" + sb.Second + "  ");
            Console.WriteLine();
        }

        //***********************************************************************
        // Puts the domino on the table
        //***********************************************************************
        static private bool SetBone(SBone sb, bool blnEnd)
        {
            SBone sbT;
            if (blnEnd)
            {
                sbT = lGame[lGame.Count - 1];
                if(sbT.Second == sb.First)
                {   
                    lGame.Add(sb);
                    return true;
                }
                else if (sbT.Second == sb.Second)
                {
                    sb.Exchange();
                    lGame.Add(sb);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                sbT = lGame[0];
                if(sbT.First == sb.Second)
                {   
                    lGame.Insert(0, sb);
                    return true;
                }
                else if (sbT.First == sb.First)
                {
                    sb.Exchange();
                    lGame.Insert(0, sb);
                    return true;
                }
                else
                    return false;
            }
        }

        static void Main(string[] args)
        {
            // whose turn is it
            bool blnFirst;
            // result of current move = TRUE, if move is done
            bool blnFRes, blnSRes;
            // endgame sign
            EFinish efFinish = EFinish.Play;
            // messages about game result
            string[] arrFinishMsg = {"---", "First player won!", "Secon player won!", "Lockdown!"};
            // number of bones in the boneyard to define whether move is correct
            int intBoneyard =0;
            // bone to make a move with
            SBone sb;
            // where to make a move
            bool blnEnd;
    
            // game initialization
            Initialize();
            // handing out dominoes at the beginning of the game
            GetHands();
             // the first bone is the first from the boneyard
            // determine at random the bone from the boneyard
            int intN = rnd.Next(lBoneyard.Count - 1);
            lGame.Add(lBoneyard[intN]);
            lBoneyard.RemoveAt(intN);
            // вывод на экран начального состояния игры
            Console.WriteLine("*************GAME STARTED*********************");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("*************Step #0");
            Console.ForegroundColor = ConsoleColor.White;
            PrintAll(lGame);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Player " + MFPlayer.PlayerName);
            MFPlayer.PrintAll();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Player " + MSPlayer.PlayerName);
            MSPlayer.PrintAll();
            Console.ReadKey();

            blnFRes = true;
            blnSRes = true;
            // first player makes first step
            blnFirst = false;
        
            intBoneyard = lBoneyard.Count;
            //-----------------------------------------------------------------
            // GAME
            do
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.White;

                // who makes a move? ---- First player
                if (blnFirst)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine("*************Step #" + intGameStep + " " + MFPlayer.PlayerName);
                    Console.ForegroundColor = ConsoleColor.White;
                    // num of bones taken
                    intLastTaken = intTaken;
                    intTaken = 0;
                    // first player move
                    intBoneyard = lBoneyard.Count;
                    blnFRes = MFPlayer.MakeStep(out sb, out blnEnd);
                    // if move had been made
                    if (blnFRes)
                    {
                        // set the bone
                        if (SetBone(sb, blnEnd) == false)
                        {
                            Console.WriteLine("!!!!!!!!Cheating!!!!!! " + MFPlayer.PlayerName);
                            Console.ReadLine();
                            return;
                        }
                    }
                    // if no move has been made
                    else if(intBoneyard == lBoneyard.Count && intBoneyard > 0)
                    {
                        Console.WriteLine("!!!!!!!!Cheating!!!!!! " + MFPlayer.PlayerName);
                        Console.ReadLine();
                        return;
                    }

                    if (blnFRes == false && blnSRes == false)
                    // lockdown
                        efFinish = EFinish.Lockdown;
                    else if (blnFRes == true)
                        // if there's no bones left, i'm the winner
                        if (MFPlayer.GetCount() == 0) efFinish = EFinish.First;
                }
                // who moves? ---- Second player
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine("*************Step #" + intGameStep + " " + MSPlayer.PlayerName);
                    Console.ForegroundColor = ConsoleColor.White;

                    // num of bones taken
                    intLastTaken = intTaken;
                    intTaken = 0;
                    // move of the first player
                    intBoneyard = lBoneyard.Count;
                    blnSRes = MSPlayer.MakeStep(out sb, out blnEnd);
                    // if move has been made
                    if (blnSRes)
                    {
                        // set the bone
                        if (SetBone(sb, blnEnd) == false)
                        {
                            Console.WriteLine("!!!!!!!!Cheating!!!!!! " + MSPlayer.PlayerName);
                            Console.ReadLine();
                            return;
                        }
                    }
                        // if no move has been made
                    else if(intBoneyard == lBoneyard.Count && intBoneyard > 0)
                    {
                        Console.WriteLine("!!!!!!!!Chaeting!!!!!! " + MSPlayer.PlayerName);
                        Console.ReadLine();
                        return;
                    }

                    if (blnFRes == false && blnSRes == false)
                        // lockdown
                        efFinish = EFinish.Lockdown;
                    else if (blnSRes == true)
                        // if there's no domino, i'm the winner
                        if (MSPlayer.GetCount() == 0) efFinish = EFinish.First;
                }
            // Printing game data after the game is finished--------------------------------------------------------
            PrintAll(lGame);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("PLAYER " + MFPlayer.PlayerName);
            MFPlayer.PrintAll();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("PLAYER " + MSPlayer.PlayerName);
            MSPlayer.PrintAll();
            
            blnFirst = ! blnFirst;
            intBoneyard = lBoneyard.Count;
            intGameStep += 1;
        }
        while(efFinish == EFinish.Play);
        // result of the current game
        Console.WriteLine(arrFinishMsg[(int) efFinish]);
        Console.WriteLine("SCORE -- " + MFPlayer.GetScore() + ":" + MSPlayer.GetScore());
        Console.ReadLine();
        }
    }
}
