import { SingleItem} from '@sitecore/ma-core';

export class SendPromoEmailActivity extends SingleItem {
	
    getVisual(): string {
        const subTitle = 'Send promo email';
        const cssClass = this.isDefined ? '' : 'undefined';
        
        return `
            <div class="viewport-readonly-editor marketing-action ${cssClass}">
                <span class="icon">
                    <img src="/~/icon/OfficeWhite/32x32/mail_forward.png" />
                </span>
                <p class="text with-subtitle" title="Send Promo Email">
                    Send Promo Email
                    <small class="subtitle" title="${subTitle}">${subTitle}</small>
                </p>
            </div>
        `;
    }

    get isDefined(): boolean {
        return true;
    }
}