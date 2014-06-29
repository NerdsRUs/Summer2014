using UnityEngine;
using System.Collections;

public class TimelineEventHide : TimelineEvent
{
	public Transform hideObject;
	public bool showOnEnd = true;

	public bool hide = true;

	public override bool initialize(float lengthOffset = 0)
	{
		/*if (hide)
		{
			Common.hideObject(hideObject);
		}
		else
		{
			Common.showObject(hideObject);
		}*/

		return base.initialize(lengthOffset);
	}

	public override bool end()
	{
		if (showOnEnd)
		{
			/*if (!hide)
			{
				Common.hideObject(hideObject);
			}
			else
			{
				Common.showObject(hideObject);
			}*/
		}

		return base.end();
	}
}
