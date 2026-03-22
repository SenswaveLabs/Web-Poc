window.googleLogin = (clientId) => {
    return new Promise((resolve) => {
        try {
            if (!window.google || !google.accounts || !google.accounts.id) {
                resolve("");
                return;
            }

            google.accounts.id.initialize({
                client_id: clientId,
                callback: (response) => {
                    try {
                        if (response && response.credential) {
                            resolve(response.credential);
                        } else {
                            resolve("");
                        }
                    } catch {
                        resolve("");
                    }
                }
            });

            google.accounts.id.prompt((notification) => {
                try {
                    if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
                        resolve("");
                    }
                } catch {
                    resolve("");
                }
            });

        } catch {
            resolve("");
        }
    });
};