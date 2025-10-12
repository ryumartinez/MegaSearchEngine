// ApplicationLayout.tsx
import * as React from 'react'
import { useRouterState, useNavigate } from '@tanstack/react-router'

import { Avatar } from '@/components/avatar'
import {
  Dropdown,
  DropdownButton,
  DropdownDivider,
  DropdownItem,
  DropdownLabel,
  DropdownMenu,
} from '@/components/dropdown'
import { Navbar, NavbarItem, NavbarSection, NavbarSpacer } from '@/components/navbar'
import {
  Sidebar,
  SidebarBody,
  SidebarFooter,
  SidebarHeader,
  SidebarHeading,
  SidebarItem,
  SidebarLabel,
  SidebarSection,
  SidebarSpacer,
} from '@/components/sidebar'
import { SidebarLayout } from '@/components/sidebar-layout'
import { getEvents } from '@/data'
import {
  ArrowRightStartOnRectangleIcon,
  ChevronDownIcon,
  ChevronUpIcon,
  Cog8ToothIcon,
  LightBulbIcon,
  PlusIcon,
  ShieldCheckIcon,
  UserCircleIcon,
} from '@heroicons/react/16/solid'
import {
  Cog6ToothIcon,
  HomeIcon,
  QuestionMarkCircleIcon,
  SparklesIcon,
  Square2StackIcon,
  TicketIcon,
} from '@heroicons/react/20/solid'

// --- Router utils ------------------------------------------------------------

function usePathname() {
  const { location } = useRouterState()
  return location.pathname
}

function isActive(pathname: string, to: string, { exact = false } = {}) {
  return exact
    ? pathname === to
    : pathname === to || pathname.startsWith(to + (to.endsWith('/') ? '' : '/'))
}

// Only intercept normal left-clicks without modifiers
function onNavClick(navigate: ReturnType<typeof useNavigate>, to: string) {
  return (e: React.MouseEvent<HTMLElement>) => {
    if (
      e.defaultPrevented ||
      e.button !== 0 || // not left click
      e.metaKey || e.ctrlKey || e.altKey || e.shiftKey // new tab/window
    ) {
      return
    }
    e.preventDefault()
    navigate({ to })
  }
}

// --- Menus -------------------------------------------------------------------

function AccountDropdownMenu({ anchor }: { anchor: 'top start' | 'bottom end' }) {
  const navigate = useNavigate()
  return (
    <DropdownMenu className="min-w-64" anchor={anchor}>
      <DropdownItem href="/account" onClick={onNavClick(navigate, '/account')}>
        <UserCircleIcon />
        <DropdownLabel>My account</DropdownLabel>
      </DropdownItem>

      <DropdownDivider />

      <DropdownItem href="/privacy" onClick={onNavClick(navigate, '/privacy')}>
        <ShieldCheckIcon />
        <DropdownLabel>Privacy policy</DropdownLabel>
      </DropdownItem>
      <DropdownItem href="/feedback" onClick={onNavClick(navigate, '/feedback')}>
        <LightBulbIcon />
        <DropdownLabel>Share feedback</DropdownLabel>
      </DropdownItem>

      <DropdownDivider />

      <DropdownItem href="/login" onClick={onNavClick(navigate, '/login')}>
        <ArrowRightStartOnRectangleIcon />
        <DropdownLabel>Sign out</DropdownLabel>
      </DropdownItem>
    </DropdownMenu>
  )
}

// --- Layout ------------------------------------------------------------------

export function ApplicationLayout({
                                    events,
                                    children,
                                  }: {
  events: Awaited<ReturnType<typeof getEvents>>
  children: React.ReactNode
}) {
  const pathname = usePathname()
  const navigate = useNavigate()

  return (
    <SidebarLayout
      navbar={
        <Navbar>
          <NavbarSpacer />
          <NavbarSection>
            <Dropdown>
              <DropdownButton as={NavbarItem}>
                <Avatar src="/users/erica.jpg" square />
              </DropdownButton>
              <AccountDropdownMenu anchor="bottom end" />
            </Dropdown>
          </NavbarSection>
        </Navbar>
      }
      sidebar={
        <Sidebar>
          <SidebarHeader>
            <Dropdown>
              <DropdownButton as={SidebarItem}>
                <Avatar src="/teams/catalyst.svg" />
                <SidebarLabel>Catalyst</SidebarLabel>
                <ChevronDownIcon />
              </DropdownButton>

              <DropdownMenu className="min-w-80 lg:min-w-64" anchor="bottom start">
                <DropdownItem href="/settings" onClick={onNavClick(navigate, '/settings')}>
                  <Cog8ToothIcon />
                  <DropdownLabel>Settings</DropdownLabel>
                </DropdownItem>

                <DropdownDivider />

                <DropdownItem href="/teams/catalyst" onClick={onNavClick(navigate, '/teams/catalyst')}>
                  <Avatar slot="icon" src="/teams/catalyst.svg" />
                  <DropdownLabel>Catalyst</DropdownLabel>
                </DropdownItem>
                <DropdownItem href="/teams/big-events" onClick={onNavClick(navigate, '/teams/big-events')}>
                  <Avatar slot="icon" initials="BE" className="bg-purple-500 text-white" />
                  <DropdownLabel>Big Events</DropdownLabel>
                </DropdownItem>

                <DropdownDivider />

                <DropdownItem href="/teams/new" onClick={onNavClick(navigate, '/teams/new')}>
                  <PlusIcon />
                  <DropdownLabel>New team&hellip;</DropdownLabel>
                </DropdownItem>
              </DropdownMenu>
            </Dropdown>
          </SidebarHeader>

          <SidebarBody>
            <SidebarSection>
              <SidebarItem
                href="/"
                current={isActive(pathname, '/', { exact: true })}
                onClick={onNavClick(navigate, '/')}
              >
                <HomeIcon />
                <SidebarLabel>Home</SidebarLabel>
              </SidebarItem>

              <SidebarItem
                href="/events"
                current={isActive(pathname, '/events')}
                onClick={onNavClick(navigate, '/events')}
              >
                <Square2StackIcon />
                <SidebarLabel>Events</SidebarLabel>
              </SidebarItem>

              <SidebarItem
                href="/orders"
                current={isActive(pathname, '/orders')}
                onClick={onNavClick(navigate, '/orders')}
              >
                <TicketIcon />
                <SidebarLabel>Orders</SidebarLabel>
              </SidebarItem>

              <SidebarItem
                href="/settings"
                current={isActive(pathname, '/settings')}
                onClick={onNavClick(navigate, '/settings')}
              >
                <Cog6ToothIcon />
                <SidebarLabel>Settings</SidebarLabel>
              </SidebarItem>
            </SidebarSection>

            <SidebarSection className="max-lg:hidden">
              <SidebarHeading>Upcoming Events</SidebarHeading>
              {events.map((event) => (
                <SidebarItem
                  key={event.id}
                  href={event.url}
                  current={isActive(pathname, event.url, { exact: true })}
                  onClick={onNavClick(navigate, event.url)}
                >
                  {event.name}
                </SidebarItem>
              ))}
            </SidebarSection>

            <SidebarSpacer />

            <SidebarSection>
              <SidebarItem href="/support" onClick={onNavClick(navigate, '/support')}>
                <QuestionMarkCircleIcon />
                <SidebarLabel>Support</SidebarLabel>
              </SidebarItem>
              <SidebarItem href="/changelog" onClick={onNavClick(navigate, '/changelog')}>
                <SparklesIcon />
                <SidebarLabel>Changelog</SidebarLabel>
              </SidebarItem>
            </SidebarSection>
          </SidebarBody>

          <SidebarFooter className="max-lg:hidden">
            <Dropdown>
              <DropdownButton as={SidebarItem}>
                <span className="flex min-w-0 items-center gap-3">
                  <Avatar src="/users/erica.jpg" className="size-10" square alt="" />
                  <span className="min-w-0">
                    <span className="block truncate text-sm/5 font-medium text-zinc-950 dark:text-white">Erica</span>
                    <span className="block truncate text-xs/5 font-normal text-zinc-500 dark:text-zinc-400">
                      erica@example.com
                    </span>
                  </span>
                </span>
                <ChevronUpIcon />
              </DropdownButton>
              <AccountDropdownMenu anchor="top start" />
            </Dropdown>
          </SidebarFooter>
        </Sidebar>
      }
    >
      {children}
    </SidebarLayout>
  )
}
