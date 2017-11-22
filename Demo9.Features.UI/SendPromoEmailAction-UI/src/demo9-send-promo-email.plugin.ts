import { Plugin } from '@sitecore/ma-core';
import { SendPromoEmailActivity } from './demo9-send-promo-email/send-promo-email-activity';
import { SendPromoEmailModuleNgFactory } from '../codegen/demo9-send-promo-email/send-promo-email-module.ngfactory';
import { ReadonlyEditorComponent } from '../codegen/demo9-send-promo-email/editor/readonly-editor.component';

// Use the @Plugin decorator to define all the activities the module contains.
@Plugin({
    activityDefinitions: [
        {
            // The ID must match the ID of the activity type description definition item in the CMS.
            id: 'f8b0dffd-e3d3-4ea2-b3fa-8bfcaa4e41da', 
            activity: SendPromoEmailActivity,
            editorComponenet: ReadonlyEditorComponent,
            editorModuleFactory: SendPromoEmailModuleNgFactory
        }
    ]
})
export default class SendPromoEmailPlugin {}
