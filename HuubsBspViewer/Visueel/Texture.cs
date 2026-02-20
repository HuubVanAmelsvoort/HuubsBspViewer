using StbImageSharp;
using OpenTK.Graphics.OpenGL4;

public class Texture {
  public int Handle;
  public Texture(byte[] imageData) {
    // 1. Genereer de handle op de videokaart
    Handle = GL.GenTexture();
    GL.BindTexture(TextureTarget.Texture2D, Handle);

    // 2. Decodeer de bytes (PNG/JPG) naar ruwe pixels
    // StbImageSharp is hier erg handig voor
    StbImage.stbi_set_flip_vertically_on_load(1); // Voeg dit toe vóór ImageResult.FromMemory
    ImageResult image = ImageResult.FromMemory(imageData, ColorComponents.RedGreenBlueAlpha);

    // 3. Upload de pixels naar de GPU
    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                  image.Width, image.Height, 0, PixelFormat.Rgba,
                  PixelType.UnsignedByte, image.Data);

    // 4. Stel filters in (zodat het er niet wazig uitziet)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
  }

  public Texture(string pad) {
    Handle = GL.GenTexture();
    Gebruik();

    // Keer de afbeelding om (StbImageSharp laadt van boven naar beneden) 
    StbImage.stbi_set_flip_vertically_on_load(1);

    if (!File.Exists(pad)) {
      Console.WriteLine($"FOUT: Plaatje bestand niet gevonden op: {Path.GetFullPath(pad)}");
    } else {
      //Console.WriteLine($"Plaatje: {pad} geladen.");
    }

    //GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

    using(Stream stream = File.OpenRead(pad)) {
      ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
      GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,image.Width, image.Height, 
                    0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
    }
    //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

    // Zorg dat de textuur herhaalt (Tiling)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
  }
  public void Gebruik() {
    GL.BindTexture(TextureTarget.Texture2D, Handle);
  }
}