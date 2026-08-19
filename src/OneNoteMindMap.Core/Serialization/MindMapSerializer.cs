using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using OneNoteMindMap.Core.Model;

namespace OneNoteMindMap.Core.Serialization
{
    public static class MindMapSerializer
    {
        public static string Serialize(MindMapDocument doc)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"format\": \"{Esc(doc.Format)}\",");
            sb.AppendLine($"  \"version\": {doc.Version},");
            sb.AppendLine($"  \"id\": \"{Esc(doc.Id)}\",");
            sb.AppendLine($"  \"title\": \"{Esc(doc.Title)}\",");
            sb.AppendLine($"  \"createdAt\": \"{doc.CreatedAt:O}\",");
            sb.AppendLine($"  \"updatedAt\": \"{doc.UpdatedAt:O}\",");

            sb.AppendLine("  \"source\": {");
            sb.AppendLine($"    \"oneNotePageId\": \"{Esc(doc.Source?.OneNotePageId)}\",");
            sb.AppendLine($"    \"oneNotePageTitle\": \"{Esc(doc.Source?.OneNotePageTitle)}\",");
            sb.AppendLine($"    \"sourceType\": \"{Esc(doc.Source?.SourceType)}\"");
            sb.AppendLine("  },");

            sb.AppendLine("  \"settings\": {");
            sb.AppendLine($"    \"layout\": \"{Esc(doc.Settings?.Layout)}\",");
            sb.AppendLine($"    \"theme\": \"{Esc(doc.Settings?.Theme)}\",");
            sb.AppendLine($"    \"nodeShape\": \"{Esc(doc.Settings?.NodeShape)}\",");
            sb.AppendLine($"    \"connectionStyle\": \"{Esc(doc.Settings?.ConnectionStyle ?? "Curved")}\",");
            sb.AppendLine($"    \"endpointStyle\": \"{Esc(doc.Settings?.EndpointStyle ?? "None")}\",");
            sb.AppendLine($"    \"direction\": \"{Esc(doc.Settings?.Direction)}\",");
            sb.AppendLine($"    \"canvasZoom\": {(doc.Settings?.CanvasZoom ?? 1.0).ToString(CultureInfo.InvariantCulture)},");
            sb.AppendLine($"    \"nodeSizeMode\": \"{Esc(doc.Settings?.NodeSizeMode ?? "Fixed")}\",");
            sb.AppendLine($"    \"nodeWidth\": {(doc.Settings?.NodeWidth ?? 160).ToString(CultureInfo.InvariantCulture)},");
            sb.AppendLine($"    \"nodeHeight\": {(doc.Settings?.NodeHeight ?? 44).ToString(CultureInfo.InvariantCulture)},");
            sb.AppendLine($"    \"autoNodeMaxWidth\": {(doc.Settings?.AutoNodeMaxWidth ?? 320).ToString(CultureInfo.InvariantCulture)},");
            sb.AppendLine($"    \"horizontalGap\": {(doc.Settings?.HorizontalGap ?? 90).ToString(CultureInfo.InvariantCulture)},");
            sb.AppendLine($"    \"verticalGap\": {(doc.Settings?.VerticalGap ?? 24).ToString(CultureInfo.InvariantCulture)},");
            sb.AppendLine($"    \"levelGap\": {(doc.Settings?.LevelGap ?? 110).ToString(CultureInfo.InvariantCulture)}");
            sb.AppendLine("  },");

            sb.Append("  \"root\": ");
            WriteNode(sb, doc.Root, 2);
            sb.AppendLine();
            sb.AppendLine("}");

            return sb.ToString();
        }

        public static MindMapDocument Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                var doc = new MindMapDocument();
                int pos = 0;
                var root = ParseObject(json, ref pos);
                if (root == null) return null;

                if (root.TryGetValue("format", out var fmt)) doc.Format = fmt as string ?? doc.Format;
                if (root.TryGetValue("version", out var ver)) doc.Version = ToInt(ver);
                if (root.TryGetValue("id", out var id)) doc.Id = id as string ?? doc.Id;
                if (root.TryGetValue("title", out var title)) doc.Title = title as string;

                if (root.TryGetValue("source", out var srcObj) && srcObj is Dictionary<string, object> src)
                {
                    doc.Source = new MindMapSourceInfo();
                    if (src.TryGetValue("oneNotePageId", out var pid)) doc.Source.OneNotePageId = pid as string;
                    if (src.TryGetValue("oneNotePageTitle", out var pt)) doc.Source.OneNotePageTitle = pt as string;
                    if (src.TryGetValue("sourceType", out var st)) doc.Source.SourceType = st as string;
                }

                if (root.TryGetValue("settings", out var setObj) && setObj is Dictionary<string, object> set)
                {
                    doc.Settings = new MindMapSettings();
                    if (set.TryGetValue("layout", out var lay)) doc.Settings.Layout = lay as string ?? "RightTree";
                    if (set.TryGetValue("theme", out var thm)) doc.Settings.Theme = thm as string ?? "Default";
                    if (set.TryGetValue("nodeShape", out var shp)) doc.Settings.NodeShape = shp as string ?? "Rounded";
                    if (set.TryGetValue("connectionStyle", out var con)) doc.Settings.ConnectionStyle = con as string ?? "Curved";
                    if (set.TryGetValue("endpointStyle", out var end)) doc.Settings.EndpointStyle = end as string ?? "None";
                    if (set.TryGetValue("direction", out var dir)) doc.Settings.Direction = dir as string ?? "Right";
                    if (set.TryGetValue("canvasZoom", out var zoom)) doc.Settings.CanvasZoom = ToDouble(zoom);
                    if (set.TryGetValue("nodeSizeMode", out var sizeMode)) doc.Settings.NodeSizeMode = sizeMode as string ?? "Fixed";
                    if (set.TryGetValue("nodeWidth", out var nw)) doc.Settings.NodeWidth = ToDouble(nw);
                    if (set.TryGetValue("nodeHeight", out var nh)) doc.Settings.NodeHeight = ToDouble(nh);
                    if (set.TryGetValue("autoNodeMaxWidth", out var maxWidth)) doc.Settings.AutoNodeMaxWidth = ToDouble(maxWidth);
                    if (set.TryGetValue("horizontalGap", out var hg)) doc.Settings.HorizontalGap = ToDouble(hg);
                    if (set.TryGetValue("verticalGap", out var vg)) doc.Settings.VerticalGap = ToDouble(vg);
                    if (set.TryGetValue("levelGap", out var lg)) doc.Settings.LevelGap = ToDouble(lg);
                }

                if (root.TryGetValue("root", out var rootNode) && rootNode is Dictionary<string, object> rn)
                {
                    doc.Root = ReadNode(rn);
                }

                return doc;
            }
            catch
            {
                return null;
            }
        }

        private static void WriteNode(StringBuilder sb, MindMapNode node, int indent)
        {
            if (node == null) { sb.Append("null"); return; }

            string pad = new string(' ', indent);
            string inner = new string(' ', indent + 2);

            sb.AppendLine("{");
            sb.AppendLine($"{inner}\"id\": \"{Esc(node.Id)}\",");
            sb.AppendLine($"{inner}\"text\": \"{Esc(node.Text)}\",");
            sb.AppendLine($"{inner}\"note\": \"{Esc(node.Note)}\",");
            sb.AppendLine($"{inner}\"contentType\": \"{Esc(node.ContentType ?? "Text")}\",");
            sb.AppendLine($"{inner}\"collapsed\": {(node.Collapsed ? "true" : "false")},");
            sb.AppendLine($"{inner}\"color\": \"{Esc(node.Color)}\",");
            sb.AppendLine($"{inner}\"icon\": \"{Esc(node.Icon)}\",");
            sb.AppendLine($"{inner}\"link\": \"{Esc(node.Link)}\",");
            sb.AppendLine($"{inner}\"sourceObjectId\": \"{Esc(node.SourceObjectId)}\",");
            sb.AppendLine($"{inner}\"manualOffsetX\": {node.ManualOffsetX.ToString(CultureInfo.InvariantCulture)},");
            sb.AppendLine($"{inner}\"manualOffsetY\": {node.ManualOffsetY.ToString(CultureInfo.InvariantCulture)},");

            sb.Append($"{inner}\"children\": [");
            if (node.Children.Count > 0)
            {
                sb.AppendLine();
                for (int i = 0; i < node.Children.Count; i++)
                {
                    sb.Append($"{inner}  ");
                    WriteNode(sb, node.Children[i], indent + 4);
                    if (i < node.Children.Count - 1) sb.AppendLine(",");
                    else sb.AppendLine();
                }
                sb.AppendLine($"{inner}]");
            }
            else
            {
                sb.AppendLine("]");
            }

            sb.Append($"{pad}}}");
        }

        private static MindMapNode ReadNode(Dictionary<string, object> obj)
        {
            var node = new MindMapNode();
            if (obj.TryGetValue("id", out var id)) node.Id = id as string ?? node.Id;
            if (obj.TryGetValue("text", out var text)) node.Text = text as string ?? "";
            if (obj.TryGetValue("note", out var note)) node.Note = note as string ?? "";
            if (obj.TryGetValue("contentType", out var contentType)) node.ContentType = contentType as string ?? "Text";
            if (obj.TryGetValue("collapsed", out var col)) node.Collapsed = col is bool b && b;
            if (obj.TryGetValue("color", out var color)) node.Color = color as string ?? "";
            if (obj.TryGetValue("icon", out var icon)) node.Icon = icon as string ?? "";
            if (obj.TryGetValue("link", out var link)) node.Link = link as string ?? "";
            if (obj.TryGetValue("sourceObjectId", out var soid)) node.SourceObjectId = soid as string ?? "";
            if (obj.TryGetValue("manualOffsetX", out var offsetX)) node.ManualOffsetX = ToDouble(offsetX);
            if (obj.TryGetValue("manualOffsetY", out var offsetY)) node.ManualOffsetY = ToDouble(offsetY);

            if (obj.TryGetValue("children", out var children) && children is List<object> childList)
            {
                foreach (var child in childList)
                {
                    if (child is Dictionary<string, object> childObj)
                        node.Children.Add(ReadNode(childObj));
                }
            }

            return node;
        }

        #region Simple JSON Parser

        private static Dictionary<string, object> ParseObject(string json, ref int pos)
        {
            SkipWhitespace(json, ref pos);
            if (pos >= json.Length || json[pos] != '{') return null;
            pos++;

            var dict = new Dictionary<string, object>();
            SkipWhitespace(json, ref pos);
            if (pos < json.Length && json[pos] == '}') { pos++; return dict; }

            while (pos < json.Length)
            {
                SkipWhitespace(json, ref pos);
                string key = ParseString(json, ref pos);
                if (key == null) break;

                SkipWhitespace(json, ref pos);
                if (pos >= json.Length || json[pos] != ':') break;
                pos++;

                SkipWhitespace(json, ref pos);
                object value = ParseValue(json, ref pos);
                dict[key] = value;

                SkipWhitespace(json, ref pos);
                if (pos < json.Length && json[pos] == ',') { pos++; continue; }
                if (pos < json.Length && json[pos] == '}') { pos++; break; }
                break;
            }

            return dict;
        }

        private static List<object> ParseArray(string json, ref int pos)
        {
            if (pos >= json.Length || json[pos] != '[') return null;
            pos++;

            var list = new List<object>();
            SkipWhitespace(json, ref pos);
            if (pos < json.Length && json[pos] == ']') { pos++; return list; }

            while (pos < json.Length)
            {
                SkipWhitespace(json, ref pos);
                object value = ParseValue(json, ref pos);
                list.Add(value);

                SkipWhitespace(json, ref pos);
                if (pos < json.Length && json[pos] == ',') { pos++; continue; }
                if (pos < json.Length && json[pos] == ']') { pos++; break; }
                break;
            }

            return list;
        }

        private static object ParseValue(string json, ref int pos)
        {
            SkipWhitespace(json, ref pos);
            if (pos >= json.Length) return null;

            char c = json[pos];
            if (c == '"') return ParseString(json, ref pos);
            if (c == '{') return ParseObject(json, ref pos);
            if (c == '[') return ParseArray(json, ref pos);
            if (c == 't' || c == 'f') return ParseBool(json, ref pos);
            if (c == 'n') { pos += 4; return null; }
            return ParseNumber(json, ref pos);
        }

        private static string ParseString(string json, ref int pos)
        {
            if (pos >= json.Length || json[pos] != '"') return null;
            pos++;

            var sb = new StringBuilder();
            while (pos < json.Length)
            {
                char c = json[pos];
                if (c == '\\')
                {
                    pos++;
                    if (pos >= json.Length) break;
                    char esc = json[pos];
                    switch (esc)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (pos + 4 < json.Length)
                            {
                                string hex = json.Substring(pos + 1, 4);
                                sb.Append((char)Convert.ToInt32(hex, 16));
                                pos += 4;
                            }
                            break;
                        default: sb.Append(esc); break;
                    }
                }
                else if (c == '"')
                {
                    pos++;
                    return sb.ToString();
                }
                else
                {
                    sb.Append(c);
                }
                pos++;
            }

            return sb.ToString();
        }

        private static object ParseNumber(string json, ref int pos)
        {
            int start = pos;
            if (pos < json.Length && json[pos] == '-') pos++;
            while (pos < json.Length && (char.IsDigit(json[pos]) || json[pos] == '.' || json[pos] == 'e' || json[pos] == 'E' || json[pos] == '+' || json[pos] == '-'))
                pos++;

            string numStr = json.Substring(start, pos - start);
            if (numStr.Contains(".") || numStr.Contains("e") || numStr.Contains("E"))
            {
                if (double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                    return d;
            }
            else
            {
                if (int.TryParse(numStr, out int i)) return i;
                if (long.TryParse(numStr, out long l)) return l;
            }
            return 0;
        }

        private static bool ParseBool(string json, ref int pos)
        {
            if (json.Substring(pos, 4) == "true") { pos += 4; return true; }
            if (json.Substring(pos, 5) == "false") { pos += 5; return false; }
            return false;
        }

        private static void SkipWhitespace(string json, ref int pos)
        {
            while (pos < json.Length && char.IsWhiteSpace(json[pos])) pos++;
        }

        #endregion

        private static string Esc(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
        }

        private static int ToInt(object o)
        {
            if (o is int i) return i;
            if (o is long l) return (int)l;
            if (o is double d) return (int)d;
            if (o is string s && int.TryParse(s, out int r)) return r;
            return 0;
        }

        private static double ToDouble(object o)
        {
            if (o is double d) return d;
            if (o is int i) return i;
            if (o is long l) return l;
            if (o is string s && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double r)) return r;
            return 0;
        }
    }
}
