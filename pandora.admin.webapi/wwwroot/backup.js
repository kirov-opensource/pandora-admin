const {fetch: originalFetch} = window;
window.fetch = async (...args) => {
    let [resource, config] = args;
    //debugger;
    let response = await originalFetch(resource, config);

    // 克隆 response 对象
    const clonedResponse = response.clone();

    var text = await streamToString(clonedResponse.body);

    console.log(text);

    return response;
};

async function streamToString(stream) {
    const reader = stream.getReader();
    const decoder = new TextDecoder('utf-8');
    let result = '';

    while (true) {
        const {done, value} = await reader.read();

        if (done) {
            break;
        }

        const chunk = decoder.decode(value, {stream: true});
        result += chunk;
    }

    return result;
}