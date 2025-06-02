import {
  animate,
  state,
  style,
  transition,
  trigger,
  keyframes,
  query,
  stagger,
} from '@angular/animations';

export const slideInAnimation = trigger('slideIn', [
  transition(':enter', [
    style({ transform: 'translateY(20px)', opacity: 0 }),
    animate(
      '300ms ease-out',
      style({ transform: 'translateY(0)', opacity: 1 })
    ),
  ]),
]);

export const fadeInAnimation = trigger('fadeIn', [
  transition(':enter', [
    style({ opacity: 0 }),
    animate('200ms ease-out', style({ opacity: 1 })),
  ]),
]);

export const buttonTransformAnimation = trigger('buttonTransform', [
  state('send', style({ transform: 'rotate(0deg)' })),
  state('cancel', style({ transform: 'rotate(135deg)' })),
  transition('send <=> cancel', animate('300ms ease-in-out')),
]);

export const typingIndicatorAnimation = trigger('typingIndicator', [
  transition(':enter', [
    style({ opacity: 0 }),
    animate('200ms ease-in-out', style({ opacity: 1 })),
  ]),
  transition(':leave', [animate('200ms ease-in-out', style({ opacity: 0 }))]),
]);

export const pulseAnimation = trigger('pulse', [
  state('inactive', style({ transform: 'scale(1)' })),
  state('active', style({ transform: 'scale(1.1)' })),
  transition('inactive <=> active', animate('500ms ease-in-out')),
]);

export const connectionStatusAnimation = trigger('connectionStatus', [
  state('Connected', style({ color: '#4CAF50' })), // Green
  state('Connecting', style({ color: '#FFC107' })), // Yellow
  state('Disconnected', style({ color: '#F44336' })), // Red
  transition('* <=> *', animate('300ms ease-in-out')),
]);

export const ratingAnimation = trigger('rating', [
  transition(':enter', [
    style({ transform: 'scale(0)', opacity: 0 }),
    animate('300ms ease-out', style({ transform: 'scale(1)', opacity: 1 })),
  ]),
  transition(':leave', [
    animate('200ms ease-in', style({ transform: 'scale(0)', opacity: 0 })),
  ]),
  state('active', style({ transform: 'scale(1.2)' })),
  state('inactive', style({ transform: 'scale(1)' })),
  transition('inactive <=> active', animate('200ms ease-out')),
]);

// Animation for text being typed
export const textAppearAnimation = trigger('textAppear', [
  transition('* => *', [
    query(
      ':enter',
      [
        style({ opacity: 0, color: 'var(--primary-color)' }),
        animate(
          '500ms ease-in',
          style({ opacity: 1, color: 'var(--on-surface-color)' })
        ),
      ],
      { optional: true }
    ),
  ]),
]);

// Typewriter animation for new message text
export const typewriterAnimation = trigger('typewriter', [
  transition('* => *', [
    query(
      ':enter',
      [
        style({ opacity: 0 }),
        stagger('30ms', animate('100ms ease-in', style({ opacity: 1 }))),
      ],
      { optional: true }
    ),
  ]),
]);
