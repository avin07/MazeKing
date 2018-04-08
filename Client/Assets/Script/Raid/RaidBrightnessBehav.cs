using UnityEngine;
using System.Collections;

public class RaidBrightnessBehav : MonoBehaviour
{
        Vector3[] ShadowLightRotate;
        Color32[] ShadowLightColor;
        float[] ShadowLightIntensity;
        Vector3[] FillLightRotate;
        Color32[] FillLightColor;
        float[] FillLightIntensity;

        Vector3[] FilllightCharacterRotation;
        Color32[] FilllightCharacterColor;
        float[] FilllightCharacterIntensity;

        Light fillLight;
        Light fillLightCharacter;
        Light shadowLight;

        void Start ()
        {
                //获得各种灯光参数 
                ShadowLightRotate = XMLPARSE_METHOD.ConvertToVector3Array(GlobalParams.GetString("shadowlight_rotation"));
                ShadowLightColor = XMLPARSE_METHOD.ConvertToColor32Array(GlobalParams.GetString("shadowlight_color"));
                ShadowLightIntensity = XMLPARSE_METHOD.ConvertToFloatArray(GlobalParams.GetString("shadowlight_intensity"));

                FillLightRotate = XMLPARSE_METHOD.ConvertToVector3Array(GlobalParams.GetString("filllight_rotation"));
                FillLightColor = XMLPARSE_METHOD.ConvertToColor32Array(GlobalParams.GetString("filllight_color"));
                FillLightIntensity = XMLPARSE_METHOD.ConvertToFloatArray(GlobalParams.GetString("filllight_intensity"));

                //角色灯光//
                FilllightCharacterRotation = XMLPARSE_METHOD.ConvertToVector3Array(GlobalParams.GetString("filllightcharacter_rotation"));
                FilllightCharacterColor = XMLPARSE_METHOD.ConvertToColor32Array(GlobalParams.GetString("filllightcharacter_color"));
                FilllightCharacterIntensity = XMLPARSE_METHOD.ConvertToFloatArray(GlobalParams.GetString("filllightcharacter_intensity"));
        }

        public void SetBrightLevel(DayTime mode)
        {
                switch (mode)
                {
                        case DayTime.Day:
                                break;
                        case DayTime.Dusk:
                                break;
                        case DayTime.Night:
                                break;
                }
        }
}
