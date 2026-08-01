#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ClearShopTools
{
    [MenuItem("Tools/Reset Shop Data (Xóa dữ liệu Mua Shop)")]
    public static void ResetShopData()
    {
        HeroSlot.ResetAllShopData();
        EditorUtility.DisplayDialog("Thông Báo", "Đã Reset toàn bộ dữ liệu Shop Anh Hùng thành công!", "OK");
    }
}
#endif