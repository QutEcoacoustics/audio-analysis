import java.awt.*;
import javax.swing.*;
import java.awt.image.*;
import java.awt.color.*;

public class GreyFrame extends JComponent
{
	private int width;
	private int height;
	private Dimension size;
	private BufferedImage image;
	private String title;


	public GreyFrame(int width, int height, byte[] data)
	{
		this(width, height, data, null);
	}

	public GreyFrame(int width, int height, byte[] data, String title)
	{
		this.width = width;
		this.height = height;
		this.title = title;
		size = new Dimension(width, height);

		DataBufferByte dataBuffer = new DataBufferByte(data, data.length, 0);

		PixelInterleavedSampleModel sampleModel = new PixelInterleavedSampleModel(DataBuffer.TYPE_BYTE, width, height, 1, width, new int[] {0});
		ColorSpace colourSpace = ColorSpace.getInstance(ColorSpace.CS_GRAY);
		ComponentColorModel colourModel = new ComponentColorModel(colourSpace, new int[] {8}, false, false, Transparency.OPAQUE, DataBuffer.TYPE_BYTE);

		WritableRaster raster = Raster.createWritableRaster(sampleModel, dataBuffer, null);

		image = new BufferedImage(colourModel, raster, false, null);
	}

	public Graphics2D getBufferImageGraphics()
	{
		return image.createGraphics();
	}

	public BufferedImage getBufferImage()
	{
		return image;
	}

	public Dimension getSize()
	{
		return size;
	}

	public Dimension getPreferredSize()
	{
		return size;
	}

	public void paint(Graphics g)
	{
		super.paint(g);
		if (image != null) g.drawImage(image, 0, 0, this);

		if (title != null) {
			g.setColor(Color.RED);
			g.drawString(title, 5, height - 5);
		}
	}
}
