### Polly Policy Cancellation Example

A very simple example in WPF of how to:

* Create an eternal Polly retry policy
* Cancel the Policy internally using a cancellation token (delayed)
* Cancel the Policy externally using a cancellation token (immediate)

I created this to teach myself how to use CancellationTokens with Polly, and learn more about how the Retry policies worked.