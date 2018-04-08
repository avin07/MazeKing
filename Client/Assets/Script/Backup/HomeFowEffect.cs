//using UnityEngine;


//[RequireComponent(typeof(Camera))]
//public class HomeFowEffect : MonoBehaviour
//{
//    /// <summary>
//    /// Shader used to create the effect. Should reference "Image Effects/Fog of War".
//    /// </summary>

//    public Shader shader;

//    /// <summary>
//    /// Color tint given to unexplored pixels.
//    /// </summary>

//    public Color unexploredColor = new Color(16 / 255f, 58 / 255f, 157 / 255f,1);

//    /// <summary>
//    /// Color tint given to explored (but not visible) pixels.
//    /// </summary>

//    public Color exploredColor = new Color(39 / 255f, 34 / 255f, 175 / 255f, 1);

//    public Color32 brightcolor = new Color32(255, 255, 255, 255);



//    public Color32 darkcolor = new Color32(154, 154, 154, 255);


//    Camera mCam;
//    Matrix4x4 mInverseMVP;
//    Material mMat;
//    int size;

//    /// <summary>
//    /// The camera we're working with needs depth.
//    /// </summary>

//    void OnEnable()
//    {
//        mCam = camera;
//        mCam.depthTextureMode = DepthTextureMode.Depth;

//        if (shader == null) shader = Shader.Find("Image Effects/Fog of War");
//    }

//    /// <summary>
//    /// Destroy the material when disabled.
//    /// </summary>

//    void OnDisable() { if (mMat) DestroyImmediate(mMat); }

//    /// <summary>
//    /// Automatically disable the effect if the shaders don't support it.
//    /// </summary>

//    void Start()
//    {
//        if (!SystemInfo.supportsImageEffects || !shader || !shader.isSupported)
//        {
//            enabled = false;
//        }
//        size = HomeManager.GetInst().HomeSize() + 2 * HomeManager.GetInst().GetBorderSize();
//        position = new Vector3(size * 0.5f, 0f, size * 0.5f) - new Vector3(0.5f, 0, 0.5f) - new Vector3(HomeManager.GetInst().GetBorderSize(), 0, HomeManager.GetInst().GetBorderSize());
//    }

//    public Vector3 position;

//    // Called by camera to apply image effect
//    void OnRenderImage(RenderTexture source, RenderTexture destination)
//    {

//        // Calculate the inverse modelview-projection matrix to convert screen coordinates to world coordinates
//        mInverseMVP = (mCam.projectionMatrix * mCam.worldToCameraMatrix).inverse;

//        float invScale = 1f / size;
//        //Vector3 t = new Vector3(size * 0.5f, 0f, size * 0.5f) - new Vector3(0.5f,0,0.5f);
//        float x = position.x - size * 0.5f;
//        float z = position.z - size * 0.5f;

//        if (mMat == null)
//        {
//            mMat = new Material(shader);
//            //mMat.hideFlags = HideFlags.HideAndDontSave;
//        }

//        Vector4 camPos = mCam.transform.position;


//        if (QualitySettings.antiAliasing > 0)
//        {
//            RuntimePlatform pl = Application.platform;

//            if (pl == RuntimePlatform.WindowsEditor ||
//                pl == RuntimePlatform.WindowsPlayer ||
//                pl == RuntimePlatform.WindowsWebPlayer)
//            {
//                camPos.w = 1f;
//            }
//        }

//        Vector4 p = new Vector4(-x * invScale, -z * invScale, invScale, 0);
//        mMat.SetColor("_Unexplored", unexploredColor);
//        mMat.SetColor("_Explored", exploredColor);
//        mMat.SetVector("_CamPos", camPos);
//        mMat.SetVector("_Params", p);
//        mMat.SetMatrix("_InverseMVP", mInverseMVP);
//        mMat.SetTexture("_FogTex0", HomeManager.GetInst().m_texture);

//        Graphics.Blit(source, destination, mMat);
//    }
//}