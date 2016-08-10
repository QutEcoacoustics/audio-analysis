// Otsu Thresholder Demo
// A.Greensted
// http://www.labbookpages.co.uk
// Please use however you like. I'd be happy to hear any feedback or comments.

import java.io.*;
import java.util.*;
import java.awt.*;
import java.awt.image.*;
import javax.swing.*;

public class OtsuDemo
{
	public OtsuDemo(String filename)
	{
		// Load Source image
		BufferedImage srcImage = null;

		try
		{
			File imgFile = new File(filename);
			srcImage = javax.imageio.ImageIO.read(imgFile);
		}
		catch (IOException ioE)
		{
			System.err.println(ioE);
			System.exit(1);
		}

		int width = srcImage.getWidth();
		int height = srcImage.getHeight();

		// Get raw image data
		Raster raster = srcImage.getData();
		DataBuffer buffer = raster.getDataBuffer();

		int type = buffer.getDataType();
		if (type != DataBuffer.TYPE_BYTE)
		{
			System.err.println("Wrong image data type");
			System.exit(1);
		}
		if (buffer.getNumBanks() != 1)
		{
			System.err.println("Wrong image data format");
			System.exit(1);
		}

		DataBufferByte byteBuffer = (DataBufferByte) buffer;
		byte[] srcData = byteBuffer.getData(0);

		// Sanity check image
		if (width * height  != srcData.length) {
			System.err.println("Unexpected image data size. Should be greyscale image");
			System.exit(1);
		}

		// Output Image info
		System.out.printf("Loaded image: '%s', width: %d, height: %d, num bytes: %d\n", filename, width, height, srcData.length);

		byte[] dstData = new byte[srcData.length];

		// Create Otsu Thresholder
		OtsuThresholder thresholder = new OtsuThresholder();
		int threshold = thresholder.doThreshold(srcData, dstData);

		System.out.printf("Threshold: %d\n", threshold);

		// Create GUI
		GreyFrame srcFrame = new GreyFrame(width, height, srcData);
		GreyFrame dstFrame = new GreyFrame(width, height, dstData);
		GreyFrame histFrame = createHistogramFrame(thresholder);

		JPanel infoPanel = new JPanel();
		infoPanel.add(histFrame);

		JPanel panel = new JPanel(new BorderLayout(5, 5));
		panel.setBorder(new javax.swing.border.EmptyBorder(5, 5, 5, 5));
		panel.add(infoPanel, BorderLayout.NORTH);
		panel.add(srcFrame, BorderLayout.WEST);
		panel.add(dstFrame, BorderLayout.EAST);
		panel.add(new JLabel("A.Greensted - http://www.labbookpages.co.uk", JLabel.CENTER), BorderLayout.SOUTH);

		JFrame frame = new JFrame("Blob Detection Demo");
		frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		frame.getContentPane().add(panel);
		frame.pack();
		frame.setVisible(true);

		// Save Images
		try
		{
			int dotPos = filename.lastIndexOf(".");
			String basename = filename.substring(0,dotPos);

			javax.imageio.ImageIO.write(dstFrame.getBufferImage(), "PNG", new File(basename+"_BW.png"));
			javax.imageio.ImageIO.write(histFrame.getBufferImage(), "PNG", new File(basename+"_hist.png"));
		}
		catch (IOException ioE)
		{
			System.err.println("Could not write image " + filename);
		}
	}

	private GreyFrame createHistogramFrame(OtsuThresholder thresholder)
	{
		int numPixels = 256 * 100;
		byte[] histPlotData = new byte[numPixels];

		int[] histData = thresholder.getHistData();
		int max = thresholder.getMaxLevelValue();
		int threshold = thresholder.getThreshold();

		for (int l=0 ; l<256 ; l++)
		{
			int ptr = (numPixels - 256) + l;
			int val = (100 * histData[l]) / max;

			if (l == threshold)
			{
				for (int i=0 ; i<100 ; i++, ptr-=256) histPlotData[ptr] = (byte) 128;
			}
			else
			{
				for (int i=0 ; i<100 ; i++, ptr-=256) histPlotData[ptr] = (val < i) ? (byte) 255 : 0;
			}
		}

		return new GreyFrame(256, 100, histPlotData);
	}

	public static void main(String args[])
	{
		System.out.println("Otsu Thresholder Demo - A.Greensted - http://www.labbookpages.co.uk");
		if (args.length<1) {
			System.err.println("Provide image filename");
			System.exit(1);
		}

		new OtsuDemo(args[0]);
	}
}
