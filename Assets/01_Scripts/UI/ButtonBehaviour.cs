using UnityEngine;

public class ButtonBehaviour : MonoBehaviour
{
    [SerializeField] GameObject target;
    public void BTN_Pressed()
    {
        //enable and trigger sound
    }
    public void BTN_DissableThis()
    {
        target = transform.gameObject;
        target.SetActive(false);
    }
    public void BTN_DissableModifiable()
    {
        if(target == null)
        {
            Debug.Log("This button has no assigned target: " + gameObject.name);
        }
        target.SetActive(false);
    }

    public void BTN_DissableParent()
    {
        target = transform.parent.gameObject;
        target.SetActive(false);
    }
    public void BTN_DissableGrandParent()
    {
        target = transform.parent.parent.gameObject;
        target.SetActive(false);
    }
    public void BTN_DissableChild()
    {
        target = transform.GetChild(0).gameObject;
        target.SetActive(false);
    }
    public void BTN_DissableGrandChild()
    {
        target = transform.GetChild(0).GetChild(0).gameObject;
        target.SetActive(false);
    }
}
