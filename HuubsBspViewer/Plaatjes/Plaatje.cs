using StbImageSharp;
using OpenTK.Graphics.OpenGL4;

public class Plaatje {
  public int AfhandelingPlaatje;
  public Plaatje(byte[] imageData) {
    AfhandelingPlaatje = GL.GenTexture();
    GL.BindTexture(TextureTarget.Texture2D, AfhandelingPlaatje);

    // Spiegelen voor OpenGL logica
    StbImage.stbi_set_flip_vertically_on_load(1);

    ImageResult image = ImageResult.FromMemory(imageData, ColorComponents.RedGreenBlueAlpha);
    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    // Vervang Repeat door ClampToEdge
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
  }

  public Plaatje(string pad) {
    AfhandelingPlaatje = GL.GenTexture();
    BindHetPlaatje();

    // Keer de afbeelding om (StbImageSharp laadt van boven naar beneden) 
    StbImage.stbi_set_flip_vertically_on_load(1);

    if (!File.Exists(pad)) {
      DiverseHelpFuncties.SchrijfRegel($"FOUT: Plaatje bestand niet gevonden op: {Path.GetFullPath(pad)}");
    }

    using(Stream stream = File.OpenRead(pad)) {
      ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
      GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,image.Width, image.Height, 
                    0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
    }
    // Zorg dat de textuur herhaalt (Tiling)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
  }
  public void BindHetPlaatje() {
    GL.BindTexture(TextureTarget.Texture2D, AfhandelingPlaatje);
  }
}