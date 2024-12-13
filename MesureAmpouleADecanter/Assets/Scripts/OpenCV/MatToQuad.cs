using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;

public class MatToQuad : MonoBehaviour
{
    public GameObject quadObject; // R�f�rence au Quad dans la sc�ne
    private Mat mat; // Le Mat que vous voulez afficher
    private Texture2D texture;
    private Material quadMaterial;

    void Start()
    {
        // Assurez-vous que le Quad est assign�
        if (quadObject == null)
        {
            Debug.LogError("Quad non assign�!");
            return;
        }

        // Cr�ez un nouveau mat�riau pour le Quad
        quadMaterial = new Material(Shader.Find("Unlit/Texture"));
        quadObject.GetComponent<Renderer>().material = quadMaterial;

    }

    void init(Mat newMat)
    {
        // Initialisez votre Mat ici (exemple)
        //mat = new Mat(480, 640, CvType.CV_8UC3);
        mat = new Mat(newMat.height(), newMat.width(), newMat.channels());
        // Remplissez votre Mat avec des donn�es...

        // Cr�ez une Texture2D
        texture = new Texture2D(mat.cols(), mat.rows(), TextureFormat.RGB24, false);

        init_ok = true;
    }

    bool init_ok = false;

    public void NewMatToCompute(Mat mat)
    {
        if (!mat.empty())
        {
            if (!init_ok)
                init(mat);
            this.mat = mat;
        }
    }


    void Update()
    {
        if (mat == null || mat.empty())
            return;


        // Mettez � jour votre Mat ici si n�cessaire

        // Convertissez le Mat en Texture2D
        Utils.matToTexture2D(mat, texture);

        // Appliquez la texture au mat�riau du Quad
        quadMaterial.mainTexture = texture;
    }

    void OnDestroy()
    {
        if (mat != null)
            mat.Dispose();
        if (texture != null)
            Destroy(texture);
        if (quadMaterial != null)
            Destroy(quadMaterial);
    }
}
