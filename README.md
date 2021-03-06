# Unity Volume Render
Volume Render for Unity, inlcude suface volume render and direct volume render. Volume dataset can be raw data or dicom file.

## Volume Render
- Ray-Casting
```glsl
float3 objViewDir;
float3 startPosition;
for (int i = 0; i < MAX_SAMPLE_COUNT; i++) {
    float3 uvw = startPosition + 0.5;
    if (outside) break;
    // getDensity(uvw);
    // calculate color
    // next pos
    startPosition += objViewDir * SAMPLE_STEP_SIZE;
}
```
![image](https://github.com/AdamWu/UnityVolumeRender/blob/main/images/raycast.gif)

- Phong Lighting Model
```glsl
float3 ambient = float3(ka, ka, ka) * tfColor;
float3 diffuse = float3(kd, kd, kd) * max(dot(N, L), 0) * tfColor;
float3 specular = float3(ks, ks, ks) * pow(max(dot(V, F), 0), power) * tfColor;
float3 color = ambient + diffuse + specular;
```
## Color Map

- generate alphamap and colormap
```csharp
Texture2D texture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, TextureFormat.RGBAFloat, false);
Color[] tfCols = new Color[TEXTURE_WIDTH * TEXTURE_HEIGHT];
List<TFColourControlPoint> cols;
List<TFAlphaControlPoint> alphas;
for (int iX = 0; iX < TEXTURE_WIDTH; iX++)
{
    // calculate color from cols and alphas 
    Color pixCol;
    // set value
    for (int iY = 0; iY < TEXTURE_HEIGHT; iY++)
    {
        tfCols[iX + iY * TEXTURE_WIDTH] = pixCol;
    }
}
```

![image](https://github.com/AdamWu/UnityVolumeRender/blob/main/images/colormap1.png)
![image](https://github.com/AdamWu/UnityVolumeRender/blob/main/images/colormap2.png)
![image](https://github.com/AdamWu/UnityVolumeRender/blob/main/images/colormap3.png)


## DICOM

dicom format： https://it.wikipedia.org/wiki/DICOM

fo-dicom：https://github.com/fo-dicom/fo-dicom

```csharp
FileStream fs = File.OpenRead(filepath);
DicomFile dcmFile = DicomFile.Open(fs);
DicomImage dcmImage = new DicomImage(dcmFile.Dataset);
int width = dcmImage.Width;
int height = dcmImage.Height;

// read tag info
slope = dcmFile.Dataset.Get<int>(DicomTag.RescaleSlope);
intercept = dcmFile.Dataset.Get<int>(DicomTag.RescaleIntercept);
......

// store CT values
int n = width * height;
int[] data = new int[n];
DicomPixelData header = DicomPixelData.Create(dcmFile.Dataset);
var pixelData = PixelDataFactory.Create(header, 0);
pixelData.Render(null, data);
for (int i = 0; i < n; i++)
{
    int value = data[i];
    float hounsfieldValue = value * slope + intercept;
    short ct = (short)Mathf.Clamp(hounsfieldValue, -1024.0f, 3071.0f);
}
```

> DICOM source
- http://www.osirix-viewer.com/datasets/    
- http://www.aycan.de/lp/sample-dicom-images.html
- http://www.barre.nom.fr/medical/samples/
- https://estore.merge.com/na/efilmcommunity/sample.html
- http://www.dicomlibrary.com/


## Screenshot

![image](https://github.com/AdamWu/UnityVolumeRender/blob/main/images/screenshoot1.png)
![image](https://github.com/AdamWu/UnityVolumeRender/blob/main/images/screenshoot2.png)