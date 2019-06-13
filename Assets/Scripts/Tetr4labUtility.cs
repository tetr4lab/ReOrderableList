//	Copyright© tetr4lab.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetr4lab {

	/// <summary>汎用ユーティリティ</summary>
	public static class Tetr4labUtility {

		/// <summary>範囲に整合して返す</summary>
		public static bool InArea (this float p, float min, float max) {
			return (p >= min && p < max);
		}

		/// <summary>エリア内に整合して返す</summary>
		public static bool InArea (this Vector2 pos, Rect area) {
			return (pos.x >= area.x && pos.x < area.x + area.width) && (pos.y >= area.y && pos.y < area.y + area.height);
		}

		/// <summary>エリア内に整合して返す</summary>
		public static bool InArea (this Vector3 pos, Rect area) {
			return InArea ((Vector2) pos, area);
		}

		/// <summary>範囲からの変位を返す</summary>
		public static float OutArea (this float p, float min, float max) {
			return (p < min) ? (p - min) : (p > max) ? (p - max) : 0;
		}

		/// <summary>エリアからの変位を返す</summary>
		public static Vector2 OutArea (this Vector2 pos, Rect area) {
			return new Vector2 (
				(pos.x < area.x) ? (pos.x - area.x) : (pos.x > area.x + area.width) ? (pos.x - area.x - area.width) : 0f,
				(pos.y < area.y) ? (pos.y - area.y) : (pos.y > area.y + area.height) ? (pos.y - area.y - area.height) : 0f
			);
		}
	}

}
