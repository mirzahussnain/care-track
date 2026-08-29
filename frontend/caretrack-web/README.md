# CareTrack Angular Frontend

Angular 22 SPA for the CareTrack public portfolio page and authenticated, role-aware workflow workspace.

## Run Locally

From this directory:

```powershell
npm ci
npm start
```

The development server runs at [http://localhost:4200](http://localhost:4200) and expects the API at `http://localhost:5001`. Development Entra/API public-client values are in `src/environments/environment.development.ts`; never add a client secret.

## Test and Build

```powershell
npm test -- --watch=false
npm run build
```

The production output is `dist/caretrack-web/browser`. `public/staticwebapp.config.json` supplies the SPA navigation fallback and is copied into that artifact.

## Deployment

The Azure Static Web Apps GitHub Actions workflow installs with `npm ci`, creates the production build, validates referenced artifacts, and uploads the browser output. Production is served at [https://caretrack.hussnainali.me](https://caretrack.hussnainali.me).

For backend setup, local SQL, Entra requirements, migrations, and integration tests, see the repository [local development guide](../../docs/14-local-development.md).
