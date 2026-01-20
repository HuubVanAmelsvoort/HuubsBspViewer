using StbImageSharp;
using OpenTK.Graphics.OpenGL4;

public class Texture {
  public int Handle;

  public Texture(string pad) {
    Handle = GL.GenTexture();
    GL.BindTexture(TextureTarget.Texture2D, Handle);
    // Keer de afbeelding om (StbImageSharp laadt van boven naar beneden) // is niet nodig
    //StbImage.stbi_set_flip_vertically_on_load(1);

    if (!File.Exists(pad)) {
      Console.WriteLine($"FOUT: Plaatje bestand niet gevonden op: {Path.GetFullPath(pad)}");
    } else {
      //Console.WriteLine($"Plaatje: {pad} geladen.");
    }
    using (Stream stream = File.OpenRead(pad)) {
      ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
      GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    image.Width, image.Height, 0, PixelFormat.Rgba,
                    PixelType.UnsignedByte, image.Data);
    }

    // Zorg dat de textuur herhaalt (Tiling)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
  }
  /**
   * Volgende zijn 6 vlakken voor de luchten
   */
  //public int LaadCubeMap(List<string> paden) {
  //  int handle = GL.GenTexture();
  //  GL.BindTexture(TextureTarget.TextureCubeMap, handle);

  //  for(int i = 0; i < paden.Count; i++) {
  //    using(Stream stream = File.OpenRead(paden[i])) {
  //      ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
  //      GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0,
  //          PixelInternalFormat.Rgba, image.Width, image.Height, 0,
  //          PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
  //    }
  //  }
  //  // Filters instellen
  //  GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
  //  GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

  //  return handle;
  //}
  public void Gebruik() {
    GL.BindTexture(TextureTarget.Texture2D, Handle);
  }
}