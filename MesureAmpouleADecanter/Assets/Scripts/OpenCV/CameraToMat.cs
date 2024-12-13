using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine.Events;

public class CameraToMat : MonoBehaviour
{
    public Mat cameraMat;

    public Camera targetCamera;
    
    Mat rgbMat;
    Texture2D texture2D;

    public UnityEvent<Mat> newCameraMat;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        // Créer une texture pour stocker le rendu de la caméra
        texture2D = new Texture2D(targetCamera.pixelWidth, targetCamera.pixelHeight, TextureFormat.RGBA32, false);

        // Initialiser le Mat
        rgbMat = new Mat(targetCamera.pixelHeight, targetCamera.pixelWidth, CvType.CV_8UC3);
    }

    void LateUpdate()
    {
        // Capturer le rendu de la caméra
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = targetCamera.targetTexture;
        if (renderTexture == null)
        {
            renderTexture = RenderTexture.GetTemporary(targetCamera.pixelWidth, targetCamera.pixelHeight, 24);
            targetCamera.targetTexture = renderTexture;
        }

        targetCamera.Render();

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new UnityEngine.Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        // Convertir la texture en Mat
        Utils.texture2DToMat(texture2D, rgbMat);

        // Ici, vous avez l'image de la caméra Unity dans rgbaMat
        // Vous pouvez maintenant effectuer des opérations OpenCV sur rgbaMat
        cameraMat = rgbMat.clone();
        newCameraMat.Invoke(cameraMat);

        // Restaurer le RenderTexture actif
        RenderTexture.active = currentRT;

        if (targetCamera.targetTexture == null)
        {
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }

    void OnDestroy()
    {
        if (rgbMat != null)
        {
            rgbMat.Dispose();
        }
        if (texture2D != null)
        {
            Destroy(texture2D);
        }
    }
}
