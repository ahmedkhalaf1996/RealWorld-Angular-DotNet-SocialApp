import { defineConfig } from "cypress";

export default defineConfig({
  allowCypressEnv: false,

  e2e: {
   baseUrl: "http://localhost:4200",
   viewportWidth: 1280,
    viewportHeight: 720,
    setupNodeEvents(on, config) {
    },
    specPattern: "cypress/e2e/**/*.cy.ts",
   supportFile: "cypress/support/e2e.ts",

  },
  component: {
    devServer: {
      framework: "angular",
      bundler: "webpack",
    },
    specPattern: "src/**/*.cy.ts",
    supportFile: "cypress/support/components.ts",
  },
});
