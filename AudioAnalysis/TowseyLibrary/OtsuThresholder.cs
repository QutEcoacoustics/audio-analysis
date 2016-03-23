using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Drawing;


namespace TowseyLibrary
{

    public class OtsuThresholder
    {
        private int[] histData;
        private int maxLevelValue;
        private int threshold;

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public OtsuThresholder()
        {
            histData = new int[256];
        }

        public int[] getHistData()
        {
            return histData;
        }

        public int getMaxLevelValue()
        {
            return maxLevelValue;
        }

        public int getThreshold()
        {
            return threshold;
        }

        public int CalculateThreshold(byte[] srcData, out byte[] monoData)
        {
            int ptr;

            // Clear histogram data
            // Set all values to zero
            ptr = 0;
            while (ptr < histData.Length) histData[ptr++] = 0;

            // Calculate histogram and find the level with the max value
            // Note: the max level value isn't required by the Otsu method
            ptr = 0;
            maxLevelValue = 0;
            while (ptr < srcData.Length)
            {
                int h = 0xFF & srcData[ptr];
                histData[h]++;
                if (histData[h] > maxLevelValue) maxLevelValue = histData[h];
                ptr++;
            }

            // Total number of pixels
            int total = srcData.Length;

            float sum = 0;
            for (int t = 0; t < 256; t++) sum += t * histData[t];

            float sumB = 0;
            int wB = 0;
            int wF = 0;

            float varMax = 0;
            threshold = 0;

            for (int t = 0; t < 256; t++)
            {
                wB += histData[t];					// Weight Background
                if (wB == 0) continue;

                wF = total - wB;						// Weight Foreground
                if (wF == 0) break;

                sumB += (float)(t * histData[t]);

                float mB = sumB / wB;				// Mean Background
                float mF = (sum - sumB) / wF;		// Mean Foreground

                // Calculate Between Class Variance
                float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

                // Check if new maximum found
                if (varBetween > varMax)
                {
                    varMax = varBetween;
                    threshold = t;
                }
            }

            // Apply threshold to create binary image
            monoData = new byte[total];
            if (monoData != null)
            {
                ptr = 0;
                while (ptr < srcData.Length)
                {
                    monoData[ptr] = (byte)(((0xFF & srcData[ptr]) >= threshold) ? (byte)255 : 0);
                    ptr++;
                }
            }

            return threshold;
        } //doThreshold


        // ================================================= STATIC METHODS =====================================================

        /// <summary>
        /// Go to following link for info on Otsu threshold
        /// http://www.labbookpages.co.uk/software/imgProc/otsuThreshold.html
        /// </summary>
        /// <param name="filename"></param>
        public static void OtsuThreshold(String filename)
    {

        // Load Source image
		Image srcImage = null;

		try
		{
			FileInfo imgFile = new FileInfo(filename);
			srcImage = ImageTools.ReadImage2Bitmap(imgFile.FullName);
		}
		catch (IOException ioE)
		{
            Console.Error.WriteLine(ioE);
			System.Environment.Exit(1);
		}

		int width = srcImage.Width;
		int height = srcImage.Height;

        // Get raw image data
        byte[,] M = ConvertColourImageToGreyScaleMatrix((Bitmap)srcImage);


		    // Sanity check image
		    if ((width * height)  !=  (M.GetLength(0)* M.GetLength(1)))
            {
                Console.Error.WriteLine("Unexpected image data size.");
                System.Environment.Exit(1);
		    }

            // Output Image info
            //Console.WriteLine("Loaded image: '%s', width: %d, height: %d, num bytes: %d\n", filename, width, height, srcData.Length);


        byte[] vector = DataTools.Matrix2Array(M);
        byte[] output;

        // Create Otsu Thresholder
            OtsuThresholder thresholder = new OtsuThresholder();
		int threshold = thresholder.CalculateThreshold(vector, out output);

        Console.WriteLine("Threshold: %d\n", threshold);

		// Create GUI
		//GreyFrame srcFrame = new GreyFrame(width, height, srcData);
		//GreyFrame dstFrame = new GreyFrame(width, height, dstData);
		//GreyFrame histFrame = CreateHistogramFrame(thresholder);

		//JPanel infoPanel = new JPanel();
		//infoPanel.add(histFrame);

		//JPanel panel = new JPanel(new BorderLayout(5, 5));
		//panel.setBorder(new javax.swing.border.EmptyBorder(5, 5, 5, 5));
		//panel.add(infoPanel, BorderLayout.NORTH);
		//panel.add(srcFrame, BorderLayout.WEST);
		//panel.add(dstFrame, BorderLayout.EAST);
		//panel.add(new JLabel("A.Greensted - http://www.labbookpages.co.uk", JLabel.CENTER), BorderLayout.SOUTH);

		//JFrame frame = new JFrame("Blob Detection Demo");
		//frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		//frame.getContentPane().add(panel);
		//frame.pack();
		//frame.setVisible(true);

		// Save Images
		//try
		//{
		//	int dotPos = filename.lastIndexOf(".");
		//	String basename = filename.substring(0,dotPos);

		//	javax.imageio.ImageIO.write(dstFrame.getBufferImage(), "PNG", new File(basename+"_BW.png"));
		//	javax.imageio.ImageIO.write(histFrame.getBufferImage(), "PNG", new File(basename+"_hist.png"));
		//}
		//catch (IOException ioE)
		//{
		//	Console.Error.WriteLine("Could not write image " + filename);
		//}

    }


        public static byte[,] ConvertColourImageToGreyScaleMatrix(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            byte[,] m = new byte[width, height];
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {                    
                    Color color = image.GetPixel(c, r);

                   // alpha = imData.data[i + 3];
                   // https://en.wikipedia.org/wiki/Grayscale
                   //       gray = red*0.2126  +  green*0.7152  +  blue*.0722;
                   m[r, c] = (byte)Math.Round((color.R * 0.2126) + (color.G * 0.7152) + (color.B * 0.0722));
                }

            }
            return m;
        }



    private Image CreateHistogramFrame(OtsuThresholder thresholder)
    {
        int width = 256;
        int height = 100;
        int numPixels = width * height;
        byte[] histPlotData = new byte[numPixels];

        int[] histData = thresholder.getHistData();
        int max = thresholder.getMaxLevelValue();
        int threshold = thresholder.getThreshold();
        var image = new Bitmap(width, height);


        for (int l = 0; l < width; l++)
        {
            int ptr = (numPixels - width) + l;
            int val = (100 * histData[l]) / max;

            if (l == threshold)
            {
                for (int i = 0; i < 100; i++, ptr -= width)
                        histPlotData[ptr] = (byte)128;
            }
            else
            {
                for (int i = 0; i < 100; i++, ptr -= width)
                        histPlotData[ptr] = (byte)((val < i) ? (byte)255 : 0);
            }
        }

        return image;
    }




        /// <summary>
        /// The core of the Otsu algorithm is shown here. 
        /// The input is an array of bytes, srcData that stores the greyscale image.
        /// 
        /// </summary>
        public static double OtsuThreshold(byte[] srcData)
        {
            double threshold = 0;

            int scaleResolution = 256;
            int[] histData = new int[scaleResolution];

            // Calculate histogram
            int ptr = 0;
            while (ptr < srcData.Length)
            {
                int h = 0xFF & srcData[ptr]; // convert grey scale value to an int. Index for histogram.
                histData[h]++;
                ptr++;
            }

            // Total number of pixels
            int total = srcData.Length;

            float sum = 0;
            for (int t = 0; t < scaleResolution; t++)
                sum += t * histData[t];

            float sumB = 0;
            int wB = 0;
            int wF = 0;

            float varMax = 0;
            threshold = 0;

            for (int t = 0; t < scaleResolution; t++)
            {
                wB += histData[t];               // Weight Background
                if (wB == 0) continue;

                wF = total - wB;                 // Weight Foreground
                if (wF == 0) break;

                sumB += (float)(t * histData[t]);

                float mB = sumB / wB;            // Mean Background
                float mF = (sum - sumB) / wF;    // Mean Foreground

                // Calculate Between Class Variance
                float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

                // Check if new maximum found
                if (varBetween > varMax)
                {
                    varMax = varBetween;
                    threshold = t;
                }
            } // for
            return threshold;
        } // OtsuThreshold()


    } // class OtsuThresholder
}
