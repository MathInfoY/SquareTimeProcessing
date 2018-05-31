using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Text;
using Tool;

namespace SquareTimeProcessingService
{
 
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SquareTimeProcessingService" in both code and config file together.
    public class SquareTimeProcessingService : ISquareTimeProcessingService
    {
        public void Start(string pathFile)
        {
            try
            {
//                File.Delete(@GlobalFile.Log);

// Replace les pièces au départ
                if (ToolBoard.isBoardConfig())
                {
                    RunningSquare.ResetAllCases();
                    File.AppendAllText(@GlobalFile.Log, DateTime.Now.ToString() + "\t *** New Game ***" + Environment.NewLine);
                }
                else
                {
                    // Cré l'échiquier
                    ToolBoard.CreateBoard(pathFile);
                    RunningSquare.InitRunning();
                }
            }
            catch (Exception)
            {
            }
          
        }

        public bool NewGame()
        {
            Suspend(true);

            RunningSquare.ResetAllCases();

            Suspend(false);
            
            File.AppendAllText(@GlobalFile.Log, DateTime.Now.ToString() + "\t *** New Game ***" + Environment.NewLine);

            return (true);
        }

        public bool isFirstLTSecund(byte FirstCase, byte SecundCase)
        {
            return (RunningSquare.isFirstLTSecund(FirstCase, SecundCase));
        }


        public bool SetFirstHit(byte noCase)
        {
            return(RunningSquare.SetFirstHit(noCase));
        }

        public DateTime GetFirstHit(byte noCase)
        {
            return (RunningSquare.GetFirstHit(noCase));
        }

        public void Suspend(bool isWaiting)
        {
            RunningSquare.Suspend(isWaiting);

            if (isWaiting)
            {
                RunningSquare.WaitSuspended();
                File.AppendAllText(@GlobalFile.Log, DateTime.Now.ToString() + "\t Hold ! " + Environment.NewLine);
            }
            else
            {
                RunningSquare.WaitRunning();
                File.AppendAllText(@GlobalFile.Log, DateTime.Now.ToString() + "\t Go !" + Environment.NewLine);
            }
        }
    }
}
