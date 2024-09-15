(function () {
    console.log("Initializing push notifications")

    // Note: Replace with your own key pair before deploying
    const applicationServerPublicKey = 'BLC8GOevpcpjQiLkO7JmVClQjycvTCYWm6Cq_a7wJZlstGTVZvwGFFHMYfXt6Njyvgx_GlXJeo5cSiZ1y4JOx1o';

    window.blazorPushNotifications = {
        getExistingSubscription: async () => {
            const worker = await navigator.serviceWorker.getRegistration();
            const existingSubscription = await worker.pushManager.getSubscription();

            if (!existingSubscription){
                return undefined 
            }
            console.log("existing subscription", existingSubscription);

            return {
                url: existingSubscription.endpoint,
                p256dh: arrayBufferToBase64(existingSubscription.getKey('p256dh')),
                auth: arrayBufferToBase64(existingSubscription.getKey('auth'))
            };
        },

        unsubscribe: async () => {
            const worker = await navigator.serviceWorker.getRegistration();
            const existingSubscription = await worker.pushManager.getSubscription();

            console.log("existing subscription", existingSubscription);

            // If the subscription exists, unsubscribe
            if (existingSubscription) {
                const success = await existingSubscription.unsubscribe();
                if (success) {
                    console.log('Successfully unsubscribed from push notifications.');
                    return true;
                } else {
                    console.log('Failed to unsubscribe from push notifications.');
                    return false;
                }
            } else {
                console.log('No push subscription found.');
                return false;
            }
        },
        subscribe: async () => {
            const worker = await navigator.serviceWorker.getRegistration();
            let existingSubscription = await worker.pushManager.getSubscription();

            if (!existingSubscription) {
                existingSubscription = await subscribe(worker);
            }

            if (!existingSubscription) {
                return undefined;
            }

            return {
                url: existingSubscription.endpoint,
                p256dh: arrayBufferToBase64(existingSubscription.getKey('p256dh')),
                auth: arrayBufferToBase64(existingSubscription.getKey('auth'))
            };
        },
        requestSubscription: async () => {
            const worker = await navigator.serviceWorker.getRegistration();
            const existingSubscription = await worker.pushManager.getSubscription();

            const newSubscription = await subscribe(worker);
            if (newSubscription) {
                return {
                    url: newSubscription.endpoint,
                    p256dh: arrayBufferToBase64(newSubscription.getKey('p256dh')),
                    auth: arrayBufferToBase64(newSubscription.getKey('auth'))
                };
            }


            if (!existingSubscription) {
                const newSubscription = await subscribe(worker);
                if (newSubscription) {
                    return {
                        url: newSubscription.endpoint,
                        p256dh: arrayBufferToBase64(newSubscription.getKey('p256dh')),
                        auth: arrayBufferToBase64(newSubscription.getKey('auth'))
                    };
                }
            } else {
                var res = {
                    url: existingSubscription.endpoint,
                    p256dh: arrayBufferToBase64(existingSubscription.getKey('p256dh')),
                    auth: arrayBufferToBase64(existingSubscription.getKey('auth'))
                };

                console.log("subscription result", res);
                return res;
            }
        }
    };

    async function subscribe(worker) {
        try {
            return await worker.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: applicationServerPublicKey
            });
        } catch (error) {
            if (error.name === 'NotAllowedError') {
                return null;
            }
            throw error;
        }
    }

    function arrayBufferToBase64(buffer) {
        // https://stackoverflow.com/a/9458996
        var binary = '';
        var bytes = new Uint8Array(buffer);
        var len = bytes.byteLength;
        for (var i = 0; i < len; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return window.btoa(binary);
    }
})();
