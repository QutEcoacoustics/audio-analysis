using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CFRecorder;

namespace CFRecorder
{
    public static class DataUploader
    {
        public static void Upload(Recording recording)
        {
            Upload(recording, false);
        }

        public static void Upload(Recording recording, bool silent)
        {
            //if (recording.StartTime == null)
            //{
            //    if (!silent)
            //        //MainForm.Log("Unable to upload recording - start time not specified");
            //}
            //else
            //{
            //    if (!silent)
            //        MainForm.Log("Commencing upload...");

                FileInfo file = new FileInfo(recording.GetPath());
                try
                {
                    byte[] buffer = new byte[file.Length];
                    using (FileStream input = file.OpenRead())
                        input.Read(buffer, 0, (int)file.Length);

                    QUT.Service.Service service = new QUT.Service.Service();
                    //service.Url = string.Format("http://{0}/Service.asmx", Settings.Server);                    
                    service.AddAudioReading("2201ef1f-6bda-4889-a7d0-d4791f918144", null, recording.StartTime.Value, buffer);
                    //System.Windows.Forms.MessageBox.Show("Data upload done");
                    //if (!silent)
                    //    MainForm.Log("Upload complete.");

                    File.Delete(file.FullName); // To delete the audio recording once the file is uploaded.
                }
                catch (Exception e)
                {
                    //if (!silent)
                    //    MainForm.Log("Upload failed - storing for later upload.\r\n{0}", e);'
                    
                }
            }


            }
        }
