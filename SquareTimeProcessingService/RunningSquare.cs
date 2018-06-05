using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using Tool;

namespace SquareTimeProcessingService
{
    static public class GlobalFile
    {
        static public string Log = "C:\\Chess\\WebServiceSquareTime.Log";
    }

    public class RunningSquare
    {
        static private bool m_stopRunning = false;
        static private bool m_endLooping = true;
        static private bool m_isBoardConfig = false;
        static private bool m_Suspended = true;
        static private bool m_Suspend = true;
        static private int[] m_noCase_x = new int[65]; // Centre de la case
        static private int[] m_noCase_y = new int[65]; // Centre de la case
        static private DateTime[] m_DateFirstHitNoCase = new DateTime[65];
        static private Dictionary<byte, Bitmap> m_dictColorCases = new Dictionary<byte, Bitmap>();

        static public void InitRunning()
        {            
            InitGrapheCases();            

            m_stopRunning = false;
            m_Suspended = true;
            m_Suspend = true;

            cExecute_RunningSquare();

            File.AppendAllText(@GlobalFile.Log, DateTime.Now.ToString() + "\t *** Initialization Success !! ***" + Environment.NewLine);
        }

        static private void InitGrapheCases()
        {
            Bitmap screenColorBmp = null;

            m_dictColorCases.Clear();

            for (byte i = 1; i <= 64; i++)
            {
                // couleur de la case
                screenColorBmp = ToolBoard.TakePictureCaseColor(i);
                m_dictColorCases.Add(i, screenColorBmp);
                m_DateFirstHitNoCase[i] = default(DateTime);
            }
        }

        static public void ResetAllCases()
        {
            Bitmap screenBmp = null;

            for (byte i = 1; i <= 64; i++)
            {
                screenBmp = ToolBoard.TakePictureCaseColor(i);
                m_dictColorCases[i].Dispose();
                m_dictColorCases[i] = screenBmp;
                m_DateFirstHitNoCase[i] = default(DateTime);
            }
        }

        static public short cExecute_RunningSquare()
        {
            short err = 0;

            try
            {
                System.ComponentModel.BackgroundWorker bw_RunningSQuare = new System.ComponentModel.BackgroundWorker();
                bw_RunningSQuare.DoWork += new System.ComponentModel.DoWorkEventHandler(bw_RunningSquare_DoWork);
                bw_RunningSQuare.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bw_RunningSquare_RunWorkerCompleted);
                bw_RunningSQuare.RunWorkerAsync();                
            }
            catch (Exception)
            {
               err = -1;
            }

            return (err);
        }


        static private void bw_RunningSquare_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Bitmap screenBmp = null;
            byte i = 0;
            Stopwatch watch = null; 
            byte nbCharOnLine = 0;

            while (!m_stopRunning)
            {
                if (!m_Suspend)
                {
                    m_Suspended = false;
                    if (watch != null)
                    {
                        watch.Stop();
                        watch = null;
                    }
                    for (i = 1; i <= 64; i++)
                    {
                        screenBmp = ToolBoard.TakePictureCaseColor(i);
                        if (screenBmp == null)
                            continue;

                        if (!ToolBoard.CompareBitmaps((Image)screenBmp, (Image)m_dictColorCases[i]))
                        {
                            if (m_DateFirstHitNoCase[i] == default(DateTime))
                            {
                                File.AppendAllText(@GlobalFile.Log, DateTime.Now.ToString() + "\tCase = " + i + "\t\t\t" + m_DateFirstHitNoCase[i] + Environment.NewLine);
                                m_DateFirstHitNoCase[i] = DateTime.Now;
                                m_dictColorCases[i].Dispose();
                                m_dictColorCases[i] = screenBmp;
                            }
                        }

                    }
                }
                else
                {
                    if (watch == null)
                        watch = Stopwatch.StartNew();

                    long elapsedMs = watch.ElapsedMilliseconds;
                    if (elapsedMs > 60000) // 1 minute
                    {
                        if (nbCharOnLine > 200)
                        {
                            File.AppendAllText(@GlobalFile.Log, "." + Environment.NewLine);
                            nbCharOnLine = 0;
                        }
                        else
                        {
                            File.AppendAllText(@GlobalFile.Log, ".");
                            nbCharOnLine++;
                        }
                        

                        watch.Stop();
                        watch = Stopwatch.StartNew();
                    }
                    
                    m_Suspended = true;
                }
            }
            
        }
       
        static private void bw_RunningSquare_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            m_endLooping = true;
            File.AppendAllText(@GlobalFile.Log, DateTime.Now.ToString() + "\t RunWorkerCompleted" + Environment.NewLine);
        }

        static private bool StopThread()
        {
            m_stopRunning = true;

            while (!m_endLooping)
            {
                Thread.Sleep(500);
            }

            return (true);
        }

        static public bool isFirstLTSecund(byte FirstCase,byte SecundCase)
        {
            if (DateTime.Compare(m_DateFirstHitNoCase[FirstCase], m_DateFirstHitNoCase[SecundCase]) < 0)
                return (true);

            return (false);
        }

        static public bool isBoardConfig()
        {
            return (m_isBoardConfig);
        }

        static public void Suspend(bool isRun)
        {
            m_Suspend = isRun;
        }

        static public DateTime GetFirstHit(byte nocase)
        {
            return (m_DateFirstHitNoCase[nocase]);
        }

        /*
         * Si la piece a ete deplacee il peut arriver que le Web Service n'est pas encore scannee la case et retourne 
         * la date de defaut (indiquant que la piece n'a pas ete deplacee encore). Il peut y avoir un decalage d'une seconde ou deux
         * avec le l'application Interface. La mise a jour apres que le Web Service l'ai detecté. On attends donc qu'il 
         * l'ai identifié
         */
        static public void SetFirstHit(byte nocase, DateTime def = default(DateTime))
        {
            byte count = 0;
            DateTime test = def;
           
            // Attends que la mise a jour se fasse avant de refaire la mise a jour
            while(test == def && count < 5)
            {
                if (count > 0)
                {
                    Thread.Sleep(1000);
                    File.AppendAllText(@GlobalFile.Log, DateTime.Now.ToString() + "\tWaiting Update..." + Environment.NewLine);
                } 
                test = m_DateFirstHitNoCase[nocase];
                count++;
            }

            File.AppendAllText(@GlobalFile.Log, DateTime.Now.ToString() + "\tCase = " + nocase + "\t\tXXX" + Environment.NewLine);

// Ne jamais deplace ces 3 lignes
            m_dictColorCases[nocase].Dispose();
            m_dictColorCases[nocase] = ToolBoard.TakePictureCaseColor(nocase);
            m_DateFirstHitNoCase[nocase] = def;

        }

        static public bool WaitSuspended()
        {
            while (!m_Suspended)
            {
                Thread.Sleep(100);
            }

            return (true);
        
        }

        static public bool WaitRunning()
        {
            while (m_Suspended)
            {
                Thread.Sleep(50);
            }

            return (true);
        }

    }
}
