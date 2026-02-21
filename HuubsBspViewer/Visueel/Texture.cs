using StbImageSharp;
using OpenTK.Graphics.OpenGL4;

public class Texture {
  public int Handle;
  public Texture(byte[] imageData) {
    Handle = GL.GenTexture();
    GL.BindTexture(TextureTarget.Texture2D, Handle);

    // Spiegelen voor OpenGL logica
    StbImage.stbi_set_flip_vertically_on_load(1);

    ImageResult image = ImageResult.FromMemory(imageData, ColorComponents.RedGreenBlueAlpha);
    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
    //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
    // Vervang Repeat door ClampToEdge
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
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