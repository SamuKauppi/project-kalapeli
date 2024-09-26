public class ScrollTemplateObject : ScrollObject
{
    protected override void Start()
    {
        SelectionButton.onClick.AddListener(() => DisplayTemplate.Instance.ChangeTemplate(arrayIndex));
    }
}
