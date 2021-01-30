using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Encodings.Web;

namespace AspNetTips.MvcSite.TagHelpers
{
	/// <summary>
	/// aタグのhrefで静的ファイルのパスにバージョンのハッシュを付加する
	/// </summary>
	[HtmlTargetElement(
		"a",
		Attributes = AppendVersionAttributeName + "," + HrefAttributeName,
		TagStructure = TagStructure.WithoutEndTag
	)]
	public class AnchorAppendVersionTagHelper : UrlResolutionTagHelper
	{
		private const string AppendVersionAttributeName = "asp-append-version";
		private const string HrefAttributeName = "href";

		/// <summary>
		/// タグヘルパーの初期化
		/// </summary>
		/// <param name="fileVersionProvider"><see cref="IFileVersionProvider"/></param>
		/// <param name="htmlEncoder"><see cref="HtmlEncoder"/></param>
		/// <param name="urlHelperFactory"><see cref="IUrlHelperFactory"/></param>
		[ActivatorUtilitiesConstructor]
		public AnchorAppendVersionTagHelper(
			IFileVersionProvider fileVersionProvider,
			HtmlEncoder htmlEncoder,
			IUrlHelperFactory urlHelperFactory
		) : base(urlHelperFactory, htmlEncoder)
		{
			FileVersionProvider = fileVersionProvider;
		}

		/// <inheritdoc />
		public override int Order => -1000;

		/// <summary>
		/// 対象となる静的ファイルのパス
		/// </summary>
		[HtmlAttributeName(HrefAttributeName)]
		public string Href { get; set; }

		/// <summary>
		/// ファイルのバージョンをクエリ文字列として付加するかどうかのフラグ
		/// </summary>
		/// <remarks>
		/// 値が true の時、ファイルのパスにクエリ文字列「v」としてハッシュが付加されます
		/// </remarks>
		[HtmlAttributeName(AppendVersionAttributeName)]
		public bool AppendVersion { get; set; }

		internal IFileVersionProvider FileVersionProvider { get; private set; }

		/// <inheritdoc />
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));

			if (output == null)
				throw new ArgumentNullException(nameof(output));

			output.CopyHtmlAttribute(HrefAttributeName, context);
			ProcessUrlAttribute(HrefAttributeName, output);

			if (AppendVersion)
			{
				EnsureFileVersionProvider();
				Href = output.Attributes[HrefAttributeName].Value as string;
				output.Attributes.SetAttribute(HrefAttributeName, FileVersionProvider.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, Href));
			}
		}

		private void EnsureFileVersionProvider()
		{
			if (FileVersionProvider == null)
			{
				FileVersionProvider = ViewContext.HttpContext.RequestServices.GetRequiredService<IFileVersionProvider>();
			}
		}
	}
}
