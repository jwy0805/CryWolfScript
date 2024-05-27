using UnityEngine;
using UnityEngine.UI;

public class UI_CanSpawn : MonoBehaviour
{
    [SerializeField] public Image background;
    private RectTransform _rectTransform;
    private bool _canSpawn;

    public bool CanSpawn
    {
        get => _canSpawn;
        set
        {
            _canSpawn = value;
            background.color = _canSpawn == false ? new Color(1f, 0f, 0f, 0.75f) : new Color(0f, 1f, 0f, 0.75f);
        }
    }

    private void Start()
    {
        _rectTransform = background.GetComponent<RectTransform>();
        float multiplier = 1.25f;
        _rectTransform.localScale = new Vector3(multiplier, multiplier, 1);
        CanSpawn = false;
    }

    private void Update()
    {
        
    }
}
