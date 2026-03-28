import {enableProdMode} from '@angular/core';

import {AppModule} from './app/app.module';
import { platformBrowser } from '@angular/platform-browser';
// import {environment} from './environment/environment';

// if (environment) {
//   enableProdMode();
// }

platformBrowser().bootstrapModule(AppModule)
  .catch(err => console.error(err));
