﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Jil;

namespace ReportDumpAPI.ReportPage
{
    public static class PageGenerator
    {
        private static readonly string template;



        static PageGenerator()
        {
            template = File.ReadAllText(Path.Combine(Config.ReportDataDir, "report-template.html"));
            template = PatchVersion(template);
        }



        public static string GenerateReportPage(string jsonStr)
        {
            var json = JSON.Deserialize<Dictionary<string, object>>(jsonStr);
            var html = template;

            html = PatchReportRoomID(html, json);
            html = PatchReportType(html, json);
            html = PatchReportID(html, json);
            html = PatchReportSearchTag(html, json);
            html = PatchReportContent(html, json);

            return html;
        }



        private static string PatchVersion(string data)
        {
            var ver = ThisAssembly.Git.Sha.ToUpperInvariant();

            if (ver.Length > 5)
            {
                ver = ver.Substring(0, 5);
            }

            var link = $"https://github.com/jdd-software/SOCVFinder/commit/{ThisAssembly.Git.Sha}";
            var html = $"<a href=\"{link}\">{ver}</a>";

            return data.Replace("$SERVER_VERSION$", html);
        }

        private static string PatchReportType(string html, Dictionary<string, object> json)
        {
            var isDupes = bool.Parse(json["is_filtered_duplicates"].ToString());
            return html.Replace("$REPORT_TYPE$", isDupes ? "Duplicate" : "Cherrypick");
        }

        private static string PatchReportID(string html, Dictionary<string, object> json)
        {
            return html.Replace("$REPORT_ID$", json["batch_nr"].ToString());
        }

        private static string PatchReportRoomID(string html, Dictionary<string, object> json)
        {
            return html.Replace("$REPORT_ROOM_ID$", json["room_id"].ToString());
        }

        private static string PatchReportSearchTag(string html, Dictionary<string, object> json)
        {
            var tagsStr = json["search_tag"].ToString();
            tagsStr = tagsStr.Remove(0, 1).Substring(0, tagsStr.Length - 2);
            tagsStr = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(tagsStr);

            var tagSplit = tagsStr.Split(';');

            for (var i = 0; i < tagSplit.Length; i++)
            {
                tagSplit[i] = $"<span class=\"reportTitleTag\">{tagSplit[i]}</span>";
            }

            var tags = "";

            if (tagSplit.Length == 1)
            {
                tags = tagSplit[0];
            }
            else if (tagSplit.Length == 2)
            {
                tags = tagSplit[0] + " & " + tagSplit[1];
            }
            else
            {
                for (var i = 0; i < tagSplit.Length - 2; i++)
                {
                    tags += tagSplit[i] + ", ";
                }

                tags += tagSplit[tagSplit.Length - 2] + " & " + tagSplit[tagSplit.Length - 1];
            }

            return html.Replace("$REPORT_TAG$", tags);
        }

        private static string PatchReportContent(string html, Dictionary<string, object> json)
        {
            var qsJson = json["questions"].ToString();
            var qs = JSON.Deserialize<Dictionary<string, object>[]>(qsJson);
            var list = new StringBuilder("<div>");

            for (var i = 0; i < qs.Length; i++)
            {
                list.AppendLine(GetQuestionHtml(qs[i]));

                if (i >= 0 && i < qs.Length - 1)
                {
                    list.AppendLine("<div class=\"itemSeparator\"></div>");
                }
            }

            list.AppendLine("</div>");

            return html.Replace("$REPORT_CONTENT$", list.ToString());
        }

        private static string GetQuestionHtml(Dictionary<string, object> json)
        {
            var html = new StringBuilder();
            var qID = json["question_id"];
            var title = json["title"].ToString();
            title = title.Remove(0, 1).Substring(0, title.Length - 2);
            var link = $"//stackoverflow.com/q/{qID}";
            var score = json["score"].ToString();
            var cvCount = json["close_vote_count"].ToString();
            var dvCount = json["delete_vote_count"].ToString();
            var views = json["view_count"].ToString();

            // Start of report.
            html.AppendLine("<div class=\"report\">");

            // Start of report header.
            html.AppendLine("<div class=\"reportHeader\">");

            // Report title.
            html.AppendLine("<h3 class=\"reportTitle\">");
            html.AppendLine($"<a target=\"_blank\" href=\"{link}\">");
            html.AppendLine(title);
            html.AppendLine("</a>");
            html.AppendLine("</h3>");

            // Report score.
            html.AppendLine("<div class=\"reportScore\">");
            html.AppendLine(score);
            html.AppendLine("</div>");

            // End of report header.
            html.AppendLine("</div>");

            // Start of report stats.
            html.AppendLine("<div class=\"reportStatsContainer\">");

            // Close vote count;
            html.AppendLine("<div>");
            html.AppendLine($"Views: {views}");
            html.AppendLine("</div>");

            // Close vote count;
            html.AppendLine("<div>");
            html.AppendLine($"Delete votes: {dvCount}");
            html.AppendLine("</div>");

            // Close vote count;
            html.AppendLine("<div>");
            html.AppendLine($"Close votes: {cvCount}");
            html.AppendLine("</div>");

            // End of report stats.
            html.AppendLine("</div>");

            // End of report.
            html.AppendLine("</div>");

            return html.ToString();
        }
    }
}