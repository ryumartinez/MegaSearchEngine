import * as Headless from '@headlessui/react'
import React, { forwardRef } from 'react'
import {
  Link as RouterLink,
  type LinkProps as RouterLinkProps,
} from '@tanstack/react-router'

/**
 * AppLink
 * - Preferred: pass `to="/route"` (TanStack Router)
 * - Backcompat: if only `href` is provided, it will be mapped to `to`
 * - Forwards a ref to the underlying <a>
 */
type AnchorProps = React.ComponentPropsWithoutRef<'a'>

type AppLinkProps =
  | (RouterLinkProps & AnchorProps) // normal TS Router usage
  | (AnchorProps & { href: string; to?: string }) // backcompat: href-only

export const Link = forwardRef<HTMLAnchorElement, AppLinkProps>(function Link(
  props,
  ref
) {
  // Normalize `href` -> `to` when `to` isn't provided
  const normalized =
    'href' in props && !('to' in props) && typeof props.href === 'string'
      ? ({ ...props, to: props.href } as RouterLinkProps & AnchorProps)
      : (props as RouterLinkProps & AnchorProps)

  return (
    <Headless.DataInteractive>
      <RouterLink {...normalized} ref={ref} />
    </Headless.DataInteractive>
  )
})

