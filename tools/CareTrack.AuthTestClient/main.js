import {
    PublicClientApplication
} from "@azure/msal-browser";

const tenantId =import.meta.env.VITE_ENTRA_TENANT_ID;

const devClientId =import.meta.env.VITE_ENTRA_DEV_CLIENT_ID;

const careTrackApiClientId = import.meta.env.VITE_CARETRACK_API_CLIENT_ID;

const msalConfig = {
    auth: {
        clientId: devClientId,

        authority:
            `https://login.microsoftonline.com/${tenantId}`,

        redirectUri:
            "http://localhost:5173/redirect.html"
    },

    cache: {
        cacheLocation:
            "sessionStorage"
    }
};

function decodeJwtPayload(token) {
    const parts = token.split(".");

    if (parts.length !== 3) {
        throw new Error("Token is not a valid JWT.");
    }

    const base64Url = parts[1];

    const base64 =
        base64Url
            .replace(/-/g, "+")
            .replace(/_/g, "/");

    const padded =
        base64.padEnd(
            base64.length + (4 - base64.length % 4) % 4,
            "="
        );

    const json =
        decodeURIComponent(
            atob(padded)
                .split("")
                .map(
                    char =>
                        "%" +
                        char
                            .charCodeAt(0)
                            .toString(16)
                            .padStart(2, "0")
                )
                .join("")
        );

    return JSON.parse(json);
}

const msalInstance =
    new PublicClientApplication(
        msalConfig);

await msalInstance.initialize();

const loginRequest = {
    scopes: [
        `api://${careTrackApiClientId}/access_as_user`
    ],
     redirectUri:
        "http://localhost:5173/redirect.html"
};

document
    .getElementById("login")
    .addEventListener(
        "click",
        async () =>
        {
            const output =
                document.getElementById(
                    "output");

            try
            {
                const response =
                    await msalInstance
                        .loginPopup(
                            loginRequest);

               const accessToken =
    response.accessToken;

const claims =
    decodeJwtPayload(accessToken);

output.textContent =
    [
        "Access token acquired ✅",
        "",
        `aud: ${claims.aud ?? "(missing)"}`,
        `iss: ${claims.iss ?? "(missing)"}`,
        `tid: ${claims.tid ?? "(missing)"}`,
        `scp: ${claims.scp ?? "(missing)"}`,
        `roles: ${
            Array.isArray(claims.roles)
                ? claims.roles.join(", ")
                : "(none)"
        }`,
        `name: ${claims.name ?? "(missing)"}`,
        `preferred_username: ${
            claims.preferred_username ?? "(missing)"
        }`,
        `oid: ${claims.oid ?? "(missing)"}`,
        `exp: ${
            claims.exp
                ? new Date(
                    claims.exp * 1000
                ).toISOString()
                : "(missing)"
        }`,
        "",
        "Raw access token:",
        accessToken
    ].join("\n");
            }
            catch (error)
            {
                console.error(error);

                output.textContent =
                    error?.message ??
                    String(error);
            }
        });