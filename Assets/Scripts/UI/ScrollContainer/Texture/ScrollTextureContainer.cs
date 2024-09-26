using UnityEngine;

public class ScrollTextureContainer : ScrollContainer
{
    public void CreateTextureContentToSprite(Texture2D[] textures)
    {
        Sprite[] sprites = new Sprite[textures.Length];
        for (int i = 0; i < textures.Length; i++)
        {
            sprites[i] = Sprite.Create(textures[i],
                                       new Rect(0, 0, -textures[i].width, textures[i].height),
                                       new Vector2(0.5f, 0.5f));
        }
        CreateContent(sprites);
    }

    public void UpdateColors(Color baseC, Color texC)
    {
        for (int i = 0; i < contentImages.Length; i++)
        {
            var textureContainer = contentImages[i] as ScrollTextureObject;

            if (textureContainer)
            {
                textureContainer.displayImage.color = texC;
                textureContainer.backgroundImage.color = baseC;
            }
        }
    }
}
