using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace BezmicanZehir.Core
{
    /// <summary>
    /// This class is used to paint on surface during run-time.
    /// </summary>
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
        [SerializeField] private Color paintColour;

        [Header("UI Fields")] 
        [SerializeField] private RectTransform sliderParent;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text percentageText;


        private int _paintedPercentage;
        private Texture2D _currentTexture;
        private Color32[] _colors;
        private MeshRenderer _paintableMeshRenderer;
        private bool _canPaint;
        private WaitForSeconds _shortDelay;

        public UnityEvent paintSceneFinish;
    
        private void Start()
        {
            _canPaint = false;
            _paintableMeshRenderer = GetComponent<MeshRenderer>();
            _currentTexture = new Texture2D(textureWidth, textureHeight);
            _colors = brushTexture.GetPixels32();

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
                _paintedPercentage = GetPaintedPercentage(_currentTexture, paintColour);
                UpdateSlider(_paintedPercentage);
            }

            if (_paintedPercentage >= 99)
            {
                _paintedPercentage = 100;
                UpdateSlider(_paintedPercentage);
                paintSceneFinish?.Invoke();
                _canPaint = false;
            }
        }

        /// <summary>
        /// This function executes Paint routine.
        /// </summary>
        private void SetPaintRoutine()
        {
            StartCoroutine(ExecutePaint());
        }

        /// <summary>
        /// This function mainly used for animation and smooth movements before painting event.
        /// Executes after player reaches the finish line on SinglePlayer level.
        /// Camera smoothly moves and rotates towards targets.
        /// </summary>
        /// <returns> Returns short delays for smooth camera movement.</returns>
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

        /// <summary>
        /// This function used to paint on surface. It instances new texture on the target surface.
        /// And manipulates texture pixels with given Input.mousePosition.
        /// </summary>
        private void PaintOnSurface()
        {
            var ray = paintCamera.ScreenPointToRay(Input.mousePosition);
        
            Physics.Raycast(ray, out var hit);
            if (!(hit.collider is null) && hit.transform.CompareTag("Paintable"))
            {
                var hitCoord = hit.textureCoord;
            
                var x = (int) ((hitCoord.x * 256) - (brushSizeAsPixel / 2.0f)); // Center X
                var y = (int) ((hitCoord.y * 256) - (brushSizeAsPixel / 2.0f)); // Center Y
            
                _currentTexture.SetPixels32(x, y, brushSizeAsPixel, brushSizeAsPixel, _colors);
                _currentTexture.Apply();

                _paintableMeshRenderer.material.mainTexture = _currentTexture;
            }
        }
    
        /// <summary>
        /// This function reads all pixels from the target Texture2D and calculates
        /// painted percentage with comparing targetColour to read colour.
        /// </summary>
        /// <param name="texture2D"> MainTexture of target painted surface.</param>
        /// <param name="targetColour"> Comparison colour.</param>
        /// <returns> Returns percentage of targetColour from target texture.</returns>
        private int GetPaintedPercentage(Texture2D texture2D, Color targetColour)
        {
            float pixelCount = texture2D.width * texture2D.height;
            if (pixelCount == 0) return 0;

            var redPixels = texture2D.GetPixels32().Where(t => t == targetColour).ToArray();
            if (redPixels.Length == 0) return 0;

            var percentage = (int) ((redPixels.Length / pixelCount) * 100);
            return percentage;
        }

        /// <summary>
        /// This function is used to visualize and show Player's progress of painting.
        /// </summary>
        /// <param name="val"> Painted percentage value.</param>
        private void UpdateSlider(int val)
        {
            slider.value = val;
            percentageText.text = val.ToString();
        }
    }
}
