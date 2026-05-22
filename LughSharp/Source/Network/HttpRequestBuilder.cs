// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ///////////////////////////////////////////////////////////////////////////////

namespace LughSharp.Source.Network;

[PublicAPI]
public class HttpRequestBuilder
{
//	/** Will be added as a prefix to each URL when {@link #url(String)} is called. Empty by default. */
//	public static String baseUrl = "";
//
//	/** Will be set for each new HttpRequest. By default set to {@code 1000}. Can be overwritten via {@link #timeout(int)}. */
//	public static int defaultTimeout = 1000;
//
//	/** Will be used for the object serialization in case {@link #jsonContent(Object)} is called. */
//	public static Json json = new Json();
//
//	private HttpRequest httpRequest;
//
//	/** Initializes the builder and sets it up to build a new {@link HttpRequest} . */
//	public HttpRequestBuilder newRequest () {
//		if (httpRequest != null) {
//			throw new IllegalStateException("A new request has already been started. Call HttpRequestBuilder.build() first.");
//		}
//
//		httpRequest = new HttpRequest();
//		httpRequest.setTimeOut(defaultTimeout);
//		return this;
//	}
//
//	/** @see HttpRequest#setMethod(String) */
//	public HttpRequestBuilder method (String httpMethod) {
//		validate();
//		httpRequest.setMethod(httpMethod);
//		return this;
//	}
//
//	/** The {@link #baseUrl} will automatically be added as a prefix to the given URL.
//	 * 
//	 * @see HttpRequest#setUrl(String) */
//	public HttpRequestBuilder url (String url) {
//		validate();
//		httpRequest.setUrl(baseUrl + url);
//		return this;
//	}
//
//	/** If this method is not called, the {@link #defaultTimeout} will be used.
//	 * 
//	 * @see HttpRequest#setTimeOut(int) */
//	public HttpRequestBuilder timeout (int timeOut) {
//		validate();
//		httpRequest.setTimeOut(timeOut);
//		return this;
//	}
//
//	/** @see HttpRequest#setFollowRedirects(boolean) */
//	public HttpRequestBuilder followRedirects (boolean followRedirects) {
//		validate();
//		httpRequest.setFollowRedirects(followRedirects);
//		return this;
//	}
//
//	/** @see HttpRequest#setIncludeCredentials(boolean) */
//	public HttpRequestBuilder includeCredentials (boolean includeCredentials) {
//		validate();
//		httpRequest.setIncludeCredentials(includeCredentials);
//		return this;
//	}
//
//	/** @see HttpRequest#setHeader(String, String) */
//	public HttpRequestBuilder header (String name, String value) {
//		validate();
//		httpRequest.setHeader(name, value);
//		return this;
//	}
//
//	/** @see HttpRequest#setContent(String) */
//	public HttpRequestBuilder content (String content) {
//		validate();
//		httpRequest.setContent(content);
//		return this;
//	}
//
//	/** @see HttpRequest#setContent(java.io.InputStream, long) */
//	public HttpRequestBuilder content (InputStream contentStream, long contentLength) {
//		validate();
//		httpRequest.setContent(contentStream, contentLength);
//		return this;
//	}
//
//	/** Sets the correct {@code ContentType} and encodes the given parameter map, then sets it as the content. */
//	public HttpRequestBuilder formEncodedContent (Map<String, String> content) {
//		validate();
//		httpRequest.setHeader(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
//		String formEncodedContent = HttpParametersUtils.convertHttpParameters(content);
//		httpRequest.setContent(formEncodedContent);
//		return this;
//	}
//
//	/** Sets the correct {@code ContentType} and encodes the given content object via {@link #json}, then sets it as the
//	 * content. */
//	public HttpRequestBuilder jsonContent (Object content) {
//		validate();
//		httpRequest.setHeader(HttpRequestHeader.ContentType, "application/json");
//		String jsonContent = json.toJson(content);
//		httpRequest.setContent(jsonContent);
//		return this;
//	}
//
//	/** Sets the {@code Authorization} header via the Base64 encoded username and password. */
//	public HttpRequestBuilder basicAuthentication (String username, String password) {
//		validate();
//		httpRequest.setHeader(HttpRequestHeader.Authorization, "Basic " + Base64Coder.encodeString(username + ":" + password));
//		return this;
//	}
//
//	/** Returns the {@link HttpRequest} that has been setup by this builder so far. After using the request, it should be returned
//	 * to the pool via {@code Pools.free(request)}. */
//	public HttpRequest build () {
//		validate();
//		HttpRequest request = httpRequest;
//		httpRequest = null;
//		return request;
//	}
//
//	private void validate () {
//		if (httpRequest == null) {
//			throw new IllegalStateException("A new request has not been started yet. Call HttpRequestBuilder.newRequest() first.");
//		}
//	}
}

// ============================================================================
// ============================================================================