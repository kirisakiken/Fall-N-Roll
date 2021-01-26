using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BezmicanZehir.Core
{
    public class Painter : MonoBehaviour
    {
        [Header("General Fields")]
        [SerializeField] private PlayerMove playerMove;
        [SerializeField] private Camera paintCamera;
        [SerializeField] private Transform cameraPositionTarget;
        [SerializeField] private Transform cameraFocus;
        
        [Header("Painting Fields")]
        [SerializeField] private int brushSizeAsPixel;
        [SerializeField] private Texture2D brushTexture;
        [SerializeField] [Min(2)] private int textureWidth;
        [SerializeField] [Min(2)] private int textureHeight;

        [Header("UI Fields")] 
        [SerializeField] private RectTransform sliderParent;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text percentageText;
        

        private Texture2D _currentTexture;
        private Color32[] _colors;
        private MeshRenderer _paintableMeshRenderer;
        private bool _canPaint;
        private WaitForSeconds _waitForFinish;
        private WaitForSeconds _shortDelay;
    
        private void Start()
        {
            _canPaint = false;
            _paintableMeshRenderer = GetComponent<MeshRenderer>();
            _currentTexture = new Texture2D(textureWidth, textureHeight);
            _colors = brushTexture.GetPixels32();

            _waitForFinish = new WaitForSeconds(1.3f);
            _shortDelay = new WaitForSeconds(0.01f);
            playerMove.executePaintRoutine += SetPaintRoutine;
        }

        private void Update()
        {
            if (!_canPaint) return;
            
            if (Input.GetMouseButton(0))
            {
                PaintOnSurface();
            }
            if (Input.GetMouseButtonUp(0))
            {
                var paintedPercentage = GetPaintedPercentage(_currentTexture, Color.red);
                UpdateSlider(paintedPercentage);
            }
        }

        private void SetPaintRoutine()
        {
            StartCoroutine(ExecutePaint());
        }

        private IEnumerator ExecutePaint()
        {
            do
            {
                paintCamera.transform.position = Vector3.MoveTowards(paintCamera.transform.position, cameraPositionTarget.position, 0.1f);
                paintCamera.transform.LookAt(cameraFocus);
                yield return _shortDelay;
            } while (Vector3.Distance(paintCamera.transform.position, cameraPositionTarget.position) > 0.1f);
            
            sliderParent.gameObject.SetActive(true);
            
            _canPaint = true;
        }

        private void PaintOnSurface()
        {
            var ray = paintCamera.ScreenPointToRay(Input.mousePosition);
        
            Physics.Raycast(ray, out var hit);
            if (!(hit.collider is null) && hit.transform.CompareTag("Paintable"))
            {
                var hitCoord = hit.textureCoord;
            
                var x = (int) ((hitCoord.x * 256) - (brushSizeAsPixel / 2.0f)); // Center x
                var y = (int) ((hitCoord.y * 256) - (brushSizeAsPixel / 2.0f)); // Center y
            
                _currentTexture.SetPixels32(x, y, brushSizeAsPixel, brushSizeAsPixel, _colors);
                _currentTexture.Apply();

                _paintableMeshRenderer.material.mainTexture = _currentTexture;
            }
        }
    
        private int GetPaintedPercentage(Texture2D texture2D, Color targetColor)
        {
            float pixelCount = texture2D.width * texture2D.height;
            if (pixelCount == 0) return 0;

            var redPixels = texture2D.GetPixels32().Where(t => t == targetColor).ToArray();
            if (redPixels.Length == 0) return 0;

            var percentage = (int) ((redPixels.Length / pixelCount) * 100);
            return percentage;
        }

        private void UpdateSlider(int val)
        {
            slider.value = val;
            percentageText.text = val.ToString();
        }
    }
}
