package org.bimserver.mail;

/******************************************************************************
 * Copyright (C) 2009-2012  BIMserver.org
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *****************************************************************************/

import java.util.Properties;

import javax.mail.Session;
import javax.mail.internet.AddressException;
import javax.mail.internet.InternetAddress;

import org.bimserver.SettingsManager;

public class MailSystem {
	private final SettingsManager settingsManager;

	public MailSystem(SettingsManager settingsManager) {
		this.settingsManager = settingsManager;
	}

	public Session createMailSession() {
		Properties props = new Properties();
		props.put("mail.smtp.host", settingsManager.getSettings().getSmtpServer());
		props.put("mail.smtp.localhost", "bimserver.org");

		Session mailSession = Session.getDefaultInstance(props);
		return mailSession;
	}

	public static boolean isValidEmailAddress(String aEmailAddress) {
		if (aEmailAddress == null) {
			return false;
		}
		try {
			new InternetAddress(aEmailAddress);
			if (!hasNameAndDomain(aEmailAddress)) {
				return false;
			}
		} catch (AddressException ex) {
			return false;
		}
		return true;
	}

	private static boolean hasNameAndDomain(String aEmailAddress) {
		String[] tokens = aEmailAddress.split("@");
		return tokens.length == 2 && !tokens[0].isEmpty() && !tokens[1].isEmpty();
	}
}